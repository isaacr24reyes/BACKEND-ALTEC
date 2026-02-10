using AltecSystem.Application.DTOs.Sales;
using MediatR;

namespace AltecSystem.Application.Commands.Sales
{
    public class CreateSaleCommand : IRequest<int>
    {
        public CreateSaleDto SaleDto { get; set; }
        public CreateSaleCommand(CreateSaleDto saleDto)
        {
            SaleDto = saleDto;
        }
    }
}
