using AltecSystem.Application.Commands.Products;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MediatR;

namespace AltecSystem.Application.Handlers.Products
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;

        public UpdateProductHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository.GetByIdAsync(request.Id);
            if (existingProduct == null)
            {
                return false; 
            }

            if (request.Categoria != null)
                existingProduct.Categoria = request.Categoria;

            if (request.Codigo != null)
                existingProduct.Codigo = request.Codigo;

            if (request.Stock.HasValue)
                existingProduct.Stock = request.Stock.Value;

            if (request.Pvp.HasValue)
                existingProduct.Pvp = request.Pvp.Value;

            if (request.PrecioMayorista.HasValue)
                existingProduct.PrecioMayorista = request.PrecioMayorista.Value;

            if (request.PrecioImportacion.HasValue)
                existingProduct.PrecioImportacion = request.PrecioImportacion.Value;

            if (request.Descripcion != null)
                existingProduct.Descripcion = request.Descripcion;

            if (request.UpdatedBy != null)
                existingProduct.CreatedBy = request.UpdatedBy;


            if (request.Foto != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Foto.FileName)}";
                var savePath = Path.Combine("wwwroot/images", fileName);

                using var stream = new FileStream(savePath, FileMode.Create);
                await request.Foto.CopyToAsync(stream);

                existingProduct.Foto = $"/images/{fileName}";

            }
            await _productRepository.UpdateAsync(existingProduct);
            return true;
        }
    }
}