using System;
using AltecSystem.Application.Commands.Sales;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MediatR;

namespace AltecSystem.Application.Handlers.Sales
{
    public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, int>
    {
        private readonly ISalesRepository _salesRepository;
        public CreateSaleCommandHandler(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<int> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            var dto = request.SaleDto;
            
            if (dto.EmployeeId.HasValue && dto.EmployeeId == Guid.Empty)
            {
                throw new ArgumentException("EmployeeId must be a valid GUID or null.");
            }

            var sale = new Sale
            {
                InvoiceNumber = dto.InvoiceNumber,
                EmployeeID = dto.EmployeeId, // Validado como GUID o null
                ProductID = dto.ProductId, // Ya es GUID
                SaleDate = dto.SaleDate,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                PaymentMethod = dto.PaymentMethod,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                Profit = dto.Profit // Agregado para incluir el campo Profit
            };
            var result = await _salesRepository.AddSaleAsync(sale);
            return result.Id;
        }
    }
}
