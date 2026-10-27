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

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
