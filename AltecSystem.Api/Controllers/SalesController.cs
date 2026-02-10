 using System.Threading.Tasks;
using AltecSystem.Application.Commands.Sales;
using AltecSystem.Application.DTOs.Sales;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AltecSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SalesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto saleDto)
        {
            if (saleDto == null)
                return BadRequest();
            var command = new CreateSaleCommand(saleDto);
            var saleId = await _mediator.Send(command);
            return CreatedAtAction(nameof(CreateSale), new { id = saleId }, saleId);
        }
    }
}
