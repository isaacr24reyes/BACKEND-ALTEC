using AltecSystem.Application.DTOs.Sales;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.Sales;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AltecSystem.Application.Handlers.Sales
{
    public class GetSalesGroupedByInvoiceNumberQueryHandler : IRequestHandler<GetSalesGroupedByInvoiceNumberQuery, List<SalesBasketDto>>
    {
        private readonly ISalesRepository _salesRepository;

        public GetSalesGroupedByInvoiceNumberQueryHandler(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<List<SalesBasketDto>> Handle(GetSalesGroupedByInvoiceNumberQuery request, CancellationToken cancellationToken)
        {
            var groupedSales = await _salesRepository.GetSalesGroupedByInvoiceNumberAsync();

            return groupedSales
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
        }
    }
}
