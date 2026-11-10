using AltecSystem.Application.DTOs.ImportedQuotations;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AltecSystem.Infrastructure.Repositories;

public class ImportedQuotationRepository : IImportedQuotationRepository
{
    private readonly AltecSystemDbContext _context;

    public ImportedQuotationRepository(AltecSystemDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ImportedQuotation quotation, CancellationToken ct = default)
    {
        // El Excel puede pesar decenas de MB (fotos incrustadas): los 30 s por defecto
        // no alcanzan para escribirlo en Azure SQL. Se amplía solo para esta operación.
        var previousTimeout = _context.Database.GetCommandTimeout();
        _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
        try
        {
            // Cabecera + filas en un solo SaveChanges (una transacción)
            await _context.ImportedQuotations.AddAsync(quotation, ct);
            await _context.SaveChangesAsync(ct);
        }
        finally
        {
            _context.Database.SetCommandTimeout(previousTimeout);
        }
    }

    public async Task<List<ImportedQuotationSummaryDto>> GetSummariesAsync(
        DateTime? desde, DateTime? hasta, string? buscar, CancellationToken ct = default)
    {
        var query = _context.ImportedQuotations.AsNoTracking();

        if (desde.HasValue)
            query = query.Where(q => q.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(q => q.Fecha <= hasta.Value);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var term = buscar.Trim();
            query = query.Where(q => q.Nombre.Contains(term) || q.NombreArchivo.Contains(term));
        }

        // Proyección sin la columna Archivo (no se descarga el binario en el listado)
        return await query
            .OrderByDescending(q => q.Fecha)
            .ThenByDescending(q => q.CreatedAt)
            .Select(q => new ImportedQuotationSummaryDto
            {
                Id = q.Id,
                Nombre = q.Nombre,
                Fecha = q.Fecha,
                NombreArchivo = q.NombreArchivo,
                TamanoBytes = q.TamanoBytes,
                TotalItems = q.TotalItems,
                TotalMonto = q.TotalMonto,
                CostosExtras = q.CostosExtras,
                CostosExtrasNota = q.CostosExtrasNota,
                CreatedAt = q.CreatedAt,
                CreatedBy = q.CreatedBy
            })
            .ToListAsync(ct);
    }

    public async Task<ImportedQuotationDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        // Cabecera sin la columna Archivo
        var detail = await _context.ImportedQuotations
            .AsNoTracking()
            .Where(q => q.Id == id)
            .Select(q => new ImportedQuotationDetailDto
            {
                Id = q.Id,
                Nombre = q.Nombre,
                Fecha = q.Fecha,
                NombreArchivo = q.NombreArchivo,
                TamanoBytes = q.TamanoBytes,
                TotalItems = q.TotalItems,
                TotalMonto = q.TotalMonto,
                CostosExtras = q.CostosExtras,
                CostosExtrasNota = q.CostosExtrasNota,
                CreatedAt = q.CreatedAt,
                CreatedBy = q.CreatedBy
            })
            .FirstOrDefaultAsync(ct);

        if (detail == null)
            return null;

        detail.Items = await _context.ImportedQuotationItems
            .AsNoTracking()
            .Where(i => i.ImportedQuotationId == id)
            .OrderBy(i => i.Fila)
            .Select(i => new ImportedQuotationItemDto
            {
                Fila = i.Fila,
                Producto = i.Producto,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Total = i.Total
            })
            .ToListAsync(ct);

        ApplyExtraCosts(detail);
        return detail;
    }

    /// <summary>
    /// Reparte los costos extras entre las filas según su valor: cada producto absorbe
    /// el mismo % de recargo sobre su costo en China (aduana, IVA o comisiones se cobran
    /// sobre el valor). Si el Excel no trae valores, se reparte por cantidad.
    /// </summary>
    private static void ApplyExtraCosts(ImportedQuotationDetailDto detail)
    {
        var extras = detail.CostosExtras;
        var valorTotal = detail.Items.Sum(i => i.Total);
        var cantidadTotal = detail.Items.Sum(i => i.Cantidad);

        foreach (var item in detail.Items)
        {
            decimal parte = 0;
            if (extras != 0)
            {
                if (valorTotal > 0)
                    parte = extras * item.Total / valorTotal;
                else if (cantidadTotal > 0)
                    parte = extras * item.Cantidad / cantidadTotal;
            }

            item.CostoEcuadorTotal = Math.Round(item.Total + parte, 4);
            item.CostoEcuadorUnitario = item.Cantidad > 0
                ? Math.Round(item.CostoEcuadorTotal / item.Cantidad, 4)
                : item.PrecioUnitario;
        }

        detail.PorcentajeRecargo = valorTotal > 0 ? Math.Round(extras / valorTotal * 100, 2) : 0;
        detail.TotalEcuador = valorTotal + extras;
    }

    public async Task<bool> UpdateExtraCostsAsync(Guid id, decimal costosExtras, string? nota, CancellationToken ct = default)
    {
        // Solo se actualizan las dos columnas: no se carga el Excel guardado
        var updated = await _context.ImportedQuotations
            .Where(q => q.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(q => q.CostosExtras, costosExtras)
                .SetProperty(q => q.CostosExtrasNota, nota), ct);
        return updated > 0;
    }

    public async Task<ImportedQuotationFileDto?> GetFileAsync(Guid id, CancellationToken ct = default)
    {
        // Leer un archivo grande también puede superar los 30 s por defecto
        _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

        return await _context.ImportedQuotations
            .AsNoTracking()
            .Where(q => q.Id == id)
            .Select(q => new ImportedQuotationFileDto
            {
                NombreArchivo = q.NombreArchivo,
                ContentType = q.ContentType,
                Archivo = q.Archivo
            })
            .FirstOrDefaultAsync(ct);
    }
}
