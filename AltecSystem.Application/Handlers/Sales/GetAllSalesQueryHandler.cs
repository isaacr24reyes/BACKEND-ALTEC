using AltecSystem.Application.DTOs.Sales;
using AltecSystem.Application.Queries.Sales;
using AltecSystem.Application.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AltecSystem.Application.Handlers.Sales
{
    public class GetAllSalesQueryHandler : IRequestHandler<GetAllSalesQuery, List<SaleDto>>
    {
        private readonly ISalesRepository _salesRepository;

        public GetAllSalesQueryHandler(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<List<SaleDto>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
        {
            var sales = await _salesRepository.GetAllSalesAsync();
            return sales;
        }
    }
}
