using AltecSystem.Application.DTOs.Sales;
using MediatR;

namespace AltecSystem.Application.Queries.Sales
{
    public class GetSalesGroupedByInvoiceNumberQuery : IRequest<List<SalesBasketDto>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
