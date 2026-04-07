using AltecSystem.Application.Commands.Quotations;
using AltecSystem.Application.DTOs.Quotations;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MediatR;

namespace AltecSystem.Application.Handlers.Quotations
{
    public class UpdateQuotationHandler : IRequestHandler<UpdateQuotationCommand, List<QuotationDetailResponse>>
    {
        private readonly IQuotationRepository _repository;

        public UpdateQuotationHandler(IQuotationRepository repository)
        {
            _repository = repository;
        }

        private static DateTime AhoraEcuador() => DateTime.UtcNow.AddHours(-5);

        public async Task<List<QuotationDetailResponse>> Handle(UpdateQuotationCommand request, CancellationToken cancellationToken)
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

            // Eliminar los detalles anteriores del mismo número
            await _repository.EliminarPorNumeroAsync(request.QuotationNumber);

            var ahora = AhoraEcuador();

            // Insertar los nuevos detalles manteniendo el mismo QuotationNumber
            var entities = request.QuotationDetails.Select(detail => new QuotationDetail
            {
                Id = Guid.NewGuid(),
                QuotationNumber = request.QuotationNumber,
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
