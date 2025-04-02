using Microsoft.AspNetCore.Mvc;
using AltecSystem.Application.Commands.Products;
using MediatR;

namespace AltecSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductCommand command)
        {
            // Verifica si el modelo es válido
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Si no es válido, devuelve un error de validación
            }

            var productId = await _mediator.Send(command);
            return Ok(new { ProductId = productId });
        }
    }
}