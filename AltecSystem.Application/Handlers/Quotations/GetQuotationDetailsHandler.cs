using AltecSystem.Application.DTOs.Quotations;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.Quotations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AltecSystem.Application.Handlers.Quotations
{
    public class GetQuotationDetailsHandler : IRequestHandler<GetQuotationDetailsQuery, List<QuotationDetailResponse>>
    {
        private readonly IQuotationRepository _repository;

        public GetQuotationDetailsHandler(IQuotationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<QuotationDetailResponse>> Handle(GetQuotationDetailsQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.QuotationNumber))
                throw new InvalidOperationException("El número de cotización no puede estar vacío.");

            var details = await _repository.ObtenerPorNumeroCotizacionAsync(request.QuotationNumber);

            if (details == null || !details.Any())
                throw new InvalidOperationException("No se encontraron detalles para el número de cotización proporcionado.");

            return details.Select(d => new QuotationDetailResponse
            {
                Id = d.Id,
                QuotationNumber = d.QuotationNumber,
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                CreatedAt = d.CreatedAt
            }).ToList();
        }
    }
}
