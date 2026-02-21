using Microsoft.AspNetCore.Mvc;
using AltecSystem.Application.Commands.Quotations;
using AltecSystem.Application.DTOs.Quotations;
using MediatR;

namespace AltecSystem.Api.Controllers
{
    [Route("api/cotizaciones")]
    [ApiController]
    public class QuotationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuotationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> SaveQuotation([FromBody] SaveQuotationCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _mediator.Send(command);
                return CreatedAtAction(nameof(SaveQuotation), response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { error = "Ocurrió un error inesperado." });
            }
        }
    }
}
