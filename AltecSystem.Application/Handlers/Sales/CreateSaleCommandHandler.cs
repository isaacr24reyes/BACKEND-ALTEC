using System.Threading;
using System.Threading.Tasks;
using AltecSystem.Application.Commands.Sales;
using AltecSystem.Application.DTOs.Sales;
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
            var sale = new Sale
            {
                InvoiceNumber = dto.InvoiceNumber,
                CustomerID = dto.CustomerID,
                EmployeeID = dto.EmployeeID,
                ProductID = dto.ProductID,
                SaleDate = dto.SaleDate,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                PaymentMethod = dto.PaymentMethod,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _salesRepository.AddSaleAsync(sale);
            return result.Id;
        }
    }
}
