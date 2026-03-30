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

        private static readonly Random _random = new Random();

        // Ecuador es UTC-5 sin horario de verano
        private static DateTime AhoraEcuador() => DateTime.UtcNow.AddHours(-5);

        public async Task<List<QuotationDetailResponse>> Handle(SaveQuotationCommand request, CancellationToken cancellationToken)
        {
            if (request.QuotationDetails == null || !request.QuotationDetails.Any())
                throw new InvalidOperationException("La lista de productos no puede estar vacía.");

            foreach (var detail in request.QuotationDetails)
            {
                if (detail.ProductId == Guid.Empty)
                    throw new InvalidOperationException("El ProductId debe ser un GUID válido.");

                if (detail.Quantity <= 0)
                    throw new InvalidOperationException("La cantidad debe ser mayor a 0.");
            }

            // Generar número único de 5 dígitos validado contra la BD
            string quotationNumber;
            int intentos = 0;
            do
            {
                quotationNumber = _random.Next(10000, 99999).ToString();
                if (++intentos > 10)
                    throw new InvalidOperationException("No se pudo generar un número único. Intente nuevamente.");
            }
            while (await _repository.ExisteAsync(quotationNumber));

            var ahora = AhoraEcuador();

            var entities = request.QuotationDetails.Select(detail => new QuotationDetail
            {
                Id = Guid.NewGuid(),
                QuotationNumber = quotationNumber,
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                PriceType = string.IsNullOrWhiteSpace(detail.PriceType) ? "pvp" : detail.PriceType,
                CreatedAt = ahora
            }).ToList();

            await _repository.GuardarAsync(entities);

            return entities.Select(e => new QuotationDetailResponse
            {
                Id = e.Id,
                QuotationNumber = e.QuotationNumber,
                ProductId = e.ProductId,
                Quantity = e.Quantity,
                UnitPrice = e.UnitPrice,
                PriceType = e.PriceType,
                CreatedAt = e.CreatedAt
            }).ToList();
        }
    }
}
