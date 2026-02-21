using Microsoft.AspNetCore.Mvc;
using AltecSystem.Application.Commands.Quotations;
using AltecSystem.Application.DTOs.Quotations;
using AltecSystem.Application.Queries.Quotations;
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

        [HttpGet("{quotationNumber}")]
        public async Task<IActionResult> GetQuotationDetails(string quotationNumber)
        {
            if (string.IsNullOrWhiteSpace(quotationNumber))
            {
                return BadRequest(new { error = "El número de cotización no puede estar vacío." });
            }

            try
            {
                var query = new GetQuotationDetailsQuery { QuotationNumber = quotationNumber };
                var response = await _mediator.Send(query);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { error = "Ocurrió un error inesperado." });
            }
        }
    }
}
