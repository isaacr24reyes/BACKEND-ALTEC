using AltecSystem.Application.DTOs.ElectronicInvoices;
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
    /// <summary>Histórico paginado con filtros.</summary>
    Task<ElectronicInvoicePageDto> ListAsync(ElectronicInvoiceFilter filter, byte ambiente, CancellationToken ct = default);

    /// <summary>Ids de facturas aún no autorizadas (firmada/recibida/error de envío) creadas desde la fecha indicada.</summary>
    Task<List<Guid>> GetPendingIdsAsync(byte ambiente, DateTimeOffset since, CancellationToken ct = default);

    /// <summary>
    /// Marca como canceladas las líneas de la venta y devuelve su stock (solo las que no estaban canceladas).
    /// No guarda: el llamador confirma todo junto con SaveChangesAsync. Devuelve cuántas líneas canceló.
    /// </summary>
    Task<int> CancelSaleAndRestoreStockAsync(string saleInvoiceNumber, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
