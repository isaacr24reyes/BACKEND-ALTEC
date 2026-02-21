using AltecSystem.Application.Commands.Sales;
using AltecSystem.Application.DTOs.Sales;
using AltecSystem.Application.Queries.Sales;
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (saleDto.EmployeeId.HasValue && saleDto.EmployeeId == Guid.Empty)
                return BadRequest(new { error = "The EmployeeId must be a valid GUID or null." });

            try
            {
                var command = new CreateSaleCommand(saleDto);
                var saleId = await _mediator.Send(command);
                return CreatedAtAction(nameof(CreateSale), new { id = saleId }, saleId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSales([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = new GetSalesGroupedByInvoiceNumberQuery
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                var groupedSales = await _mediator.Send(query);
                return Ok(groupedSales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
