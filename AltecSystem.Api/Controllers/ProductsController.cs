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

        // Endpoint para crear producto
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productId = await _mediator.Send(command);
            return Ok(new { ProductId = productId });
        }
        
        [HttpGet]
        public async Task<IActionResult> GetActiveProducts(
            int pageNumber, 
            int pageSize, 
            string? filter, 
            string? orderBy, 
            string? sortOrder)
        {
            var query = new GetActiveProductsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filter = filter,
                OrderBy = orderBy,
                SortOrder = sortOrder
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch.");

            var updatedProduct = await _mediator.Send(command);

            if (updatedProduct == null)
                return NotFound("Producto no encontrado.");

            return Ok(updatedProduct);
        }

    }
}