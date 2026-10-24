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

        return detail;
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
