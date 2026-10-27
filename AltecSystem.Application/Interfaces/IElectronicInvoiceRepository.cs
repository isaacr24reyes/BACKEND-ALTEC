using AltecSystem.Domain.Entities;

namespace AltecSystem.Application.Interfaces;

public interface IElectronicInvoiceRepository
{
    Task<SriSettings> GetSettingsAsync(CancellationToken ct = default);

    /// <summary>Reserva de forma atómica el siguiente secuencial del ambiente (1 pruebas, 2 producción).</summary>
    Task<int> ReserveSecuencialAsync(byte ambiente, CancellationToken ct = default);

    Task<Customer?> GetCustomerAsync(string identificacion, CancellationToken ct = default);
    Task<Customer> UpsertCustomerAsync(Customer customer, CancellationToken ct = default);
    Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ElectronicInvoice invoice, CancellationToken ct = default);
    Task<ElectronicInvoice?> GetAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
