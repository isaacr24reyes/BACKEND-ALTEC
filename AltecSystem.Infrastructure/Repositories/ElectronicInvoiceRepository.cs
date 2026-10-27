using AltecSystem.Application.DTOs.ElectronicInvoices;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AltecSystem.Infrastructure.Repositories;

public class ElectronicInvoiceRepository : IElectronicInvoiceRepository
{
    private readonly AltecSystemDbContext _context;

    public ElectronicInvoiceRepository(AltecSystemDbContext context)
    {
        _context = context;
    }

    public async Task<SriSettings> GetSettingsAsync(CancellationToken ct = default)
    {
        return await _context.SriSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == 1, ct)
            ?? throw new InvalidOperationException("Falta la configuración del emisor (tabla SriSettings). Ejecute Database/002_facturacion_electronica.sql.");
    }

    public async Task<int> ReserveSecuencialAsync(byte ambiente, CancellationToken ct = default)
    {
        // UPDATE ... OUTPUT es atómico: dos ventas simultáneas nunca reciben el mismo número
        var column = ambiente == 2 ? "SiguienteSecuencialProduccion" : "SiguienteSecuencialPruebas";
        var sql = $"UPDATE dbo.SriSettings SET {column} = {column} + 1, UpdatedAt = SYSDATETIMEOFFSET() " +
                  $"OUTPUT deleted.{column} AS [Value] WHERE Id = 1";

        var result = await _context.Database.SqlQueryRaw<int>(sql).ToListAsync(ct);
        if (result.Count == 0)
            throw new InvalidOperationException("No se pudo reservar el número de factura (falta la fila Id = 1 en SriSettings).");

        return result[0];
    }

    public Task<Customer?> GetCustomerAsync(string identificacion, CancellationToken ct = default)
    {
        return _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Identificacion == identificacion, ct);
    }

    public async Task<Customer> UpsertCustomerAsync(Customer customer, CancellationToken ct = default)
    {
        var existing = await _context.Customers.FirstOrDefaultAsync(c => c.Identificacion == customer.Identificacion, ct);
        if (existing == null)
        {
            await _context.Customers.AddAsync(customer, ct);
            return customer;
        }

        existing.TipoIdentificacion = customer.TipoIdentificacion;
        existing.RazonSocial = customer.RazonSocial;
        existing.Direccion = customer.Direccion ?? existing.Direccion;
        existing.Email = customer.Email ?? existing.Email;
        existing.Telefono = customer.Telefono ?? existing.Telefono;
        existing.UpdatedAt = customer.UpdatedAt;
        return existing;
    }

    public Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default)
    {
        return _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task AddAsync(ElectronicInvoice invoice, CancellationToken ct = default)
    {
        await _context.ElectronicInvoices.AddAsync(invoice, ct);
    }

    public Task<ElectronicInvoice?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return _context.ElectronicInvoices
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<ElectronicInvoicePageDto> ListAsync(ElectronicInvoiceFilter filter, byte ambiente, CancellationToken ct = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var baseQuery = _context.ElectronicInvoices.AsNoTracking().Where(i => i.Ambiente == ambiente);
        var query = baseQuery;

        if (filter.Desde.HasValue) query = query.Where(i => i.FechaEmision >= filter.Desde.Value.Date);
        if (filter.Hasta.HasValue) query = query.Where(i => i.FechaEmision <= filter.Hasta.Value.Date);

        switch (filter.Estado?.Trim().ToUpperInvariant())
        {
            case "AUTORIZADO":
                query = query.Where(i => i.Estado == SriInvoiceStates.Autorizado);
                break;
            case "PENDIENTE":
                query = query.Where(i => SriInvoiceStates.Pendientes.Contains(i.Estado));
                break;
            case "ANULADA":
                query = query.Where(i => i.Estado == SriInvoiceStates.Anulada);
                break;
            case "RECHAZADA":
                query = query.Where(i => i.Estado == SriInvoiceStates.Devuelta || i.Estado == SriInvoiceStates.NoAutorizado);
                break;
        }

        if (!string.IsNullOrWhiteSpace(filter.Buscar))
        {
            var q = filter.Buscar.Trim();
            query = query.Where(i => i.RazonSocialComprador.Contains(q) || i.IdentificacionComprador.Contains(q)
                                     || i.NumeroFactura.Contains(q) || (i.SaleInvoiceNumber != null && i.SaleInvoiceNumber.Contains(q)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(i => i.Secuencial)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ElectronicInvoiceListItemDto
            {
                Id = i.Id,
                NumeroFactura = i.NumeroFactura,
                ClaveAcceso = i.ClaveAcceso,
                Ambiente = i.Ambiente,
                FechaEmision = i.FechaEmision,
                FechaAutorizacion = i.FechaAutorizacion,
                IdentificacionComprador = i.IdentificacionComprador,
                RazonSocialComprador = i.RazonSocialComprador,
                EmailComprador = i.EmailComprador,
                ImporteTotal = i.ImporteTotal,
                Estado = i.Estado,
                EmailEnviado = i.EmailEnviado,
                SaleInvoiceNumber = i.SaleInvoiceNumber,
                FechaAnulacion = i.FechaAnulacion,
                MotivoAnulacion = i.MotivoAnulacion,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(ct);

        var pendientes = await baseQuery.CountAsync(i => SriInvoiceStates.Pendientes.Contains(i.Estado), ct);

        return new ElectronicInvoicePageDto { Items = items, Total = total, Page = page, PageSize = pageSize, Pendientes = pendientes };
    }

    public Task<List<Guid>> GetPendingIdsAsync(byte ambiente, DateTimeOffset since, CancellationToken ct = default)
    {
        return _context.ElectronicInvoices.AsNoTracking()
            .Where(i => i.Ambiente == ambiente && SriInvoiceStates.Pendientes.Contains(i.Estado) && i.CreatedAt >= since)
            .OrderBy(i => i.Secuencial)
            .Select(i => i.Id)
            .ToListAsync(ct);
    }

    public async Task<int> CancelSaleAndRestoreStockAsync(string saleInvoiceNumber, CancellationToken ct = default)
    {
        var lines = await _context.Sales
            .Where(s => s.InvoiceNumber == saleInvoiceNumber && s.Status != "Cancelled")
            .ToListAsync(ct);

        foreach (var line in lines)
        {
            line.Status = "Cancelled";
            var product = await _context.Productos.FirstOrDefaultAsync(p => p.Id == line.ProductId, ct);
            if (product != null) product.Stock += line.Quantity;
        }

        return lines.Count;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
