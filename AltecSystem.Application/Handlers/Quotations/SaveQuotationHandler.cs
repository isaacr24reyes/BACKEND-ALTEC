using AltecSystem.Application.Commands.Quotations;
using AltecSystem.Application.DTOs.Quotations;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AltecSystem.Application.Handlers.Quotations
{
    public class SaveQuotationHandler : IRequestHandler<SaveQuotationCommand, List<QuotationDetailResponse>>
    {
        private readonly IQuotationRepository _repository;

        public SaveQuotationHandler(IQuotationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<QuotationDetailResponse>> Handle(SaveQuotationCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.QuotationNumber))
                throw new InvalidOperationException("El número de cotización no puede estar vacío.");

            if (request.QuotationDetails == null || !request.QuotationDetails.Any())
                throw new InvalidOperationException("La lista de productos no puede estar vacía.");

            foreach (var detail in request.QuotationDetails)
            {
                if (detail.ProductId == Guid.Empty)
                    throw new InvalidOperationException("El ProductId debe ser un GUID válido.");

                if (detail.Quantity <= 0)
                    throw new InvalidOperationException("La cantidad debe ser mayor a 0.");
            }

            var entities = request.QuotationDetails.Select(detail => new QuotationDetail
            {
                Id = Guid.NewGuid(),
                QuotationNumber = request.QuotationNumber,
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _repository.GuardarAsync(entities);

            return entities.Select(e => new QuotationDetailResponse
            {
                Id = e.Id,
                QuotationNumber = e.QuotationNumber,
                ProductId = e.ProductId,
                Quantity = e.Quantity,
                CreatedAt = e.CreatedAt
            }).ToList();
        }
    }
}
