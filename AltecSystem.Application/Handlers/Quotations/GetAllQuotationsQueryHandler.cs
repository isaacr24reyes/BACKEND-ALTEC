using AltecSystem.Application.DTOs.Quotations;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.Quotations;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AltecSystem.Application.Handlers.Quotations
{
    public class GetAllQuotationsQueryHandler : IRequestHandler<GetAllQuotationsQuery, List<QuotationSummaryDto>>
    {
        private readonly IQuotationRepository _repository;

        public GetAllQuotationsQueryHandler(IQuotationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<QuotationSummaryDto>> Handle(GetAllQuotationsQuery request, CancellationToken cancellationToken)
        {
            var all = await _repository.ObtenerTodasAsync();

            return all
                .GroupBy(q => q.QuotationNumber)
                .Select(g => new QuotationSummaryDto
                {
                    QuotationNumber = g.Key,
                    CreatedAt = g.Min(q => q.CreatedAt),
                    ItemCount = g.Count(),
                    TotalAmount = g.Sum(q => q.UnitPrice * q.Quantity),
                    Items = g.Select(q => new QuotationDetailResponse
                    {
                        Id = q.Id,
                        QuotationNumber = q.QuotationNumber,
                        ProductId = q.ProductId,
                        Quantity = q.Quantity,
                        UnitPrice = q.UnitPrice,
                        PriceType = q.PriceType,
                        CreatedAt = q.CreatedAt
                    }).ToList()
                })
                .OrderByDescending(q => q.CreatedAt)
                .ToList();
        }
    }
}
