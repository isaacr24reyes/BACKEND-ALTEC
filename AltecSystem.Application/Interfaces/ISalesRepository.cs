using AltecSystem.Application.DTOs.Sales;

namespace AltecSystem.Application.Interfaces
{
    using Domain.Entities;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public interface ISalesRepository
    {
        Task<Sale> AddSaleAsync(Sale sale);
        Task<List<SaleDto>> GetAllSalesAsync();
        Task<List<SalesBasketDto>> GetSalesGroupedByInvoiceNumberAsync();
    }
}
