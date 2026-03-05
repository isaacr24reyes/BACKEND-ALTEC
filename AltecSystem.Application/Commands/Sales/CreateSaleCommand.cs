using AltecSystem.Application.DTOs.Sales;
using MediatR;

namespace AltecSystem.Application.Commands.Sales
{
    public class CreateSaleCommand : IRequest<int>
    {
        public CreateSaleDto SaleDto { get; set; }
        public decimal UnitPrice { get; set; } // Agregado para incluir el precio unitario
        public CreateSaleCommand(CreateSaleDto saleDto, decimal unitPrice)
        {
            SaleDto = saleDto;
            UnitPrice = unitPrice;
        }
    }
}
