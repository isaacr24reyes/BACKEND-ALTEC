using AltecSystem.Application.Commands.Products;
using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using MediatR;
using System.Globalization;

namespace AltecSystem.Application.Handlers.Products
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Product> 
    {
        private readonly IProductRepository _productRepository;

        public UpdateProductHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Product> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository.GetByIdAsync(request.Id);
            if (existingProduct == null)
            {
                return null!;
            }

            if (request.Categoria != null)
                existingProduct.Categoria = request.Categoria;

            if (request.Codigo != null)
                existingProduct.Codigo = request.Codigo;

            if (request.Stock.HasValue)
                existingProduct.Stock = request.Stock.Value;

            // ✅ Parse manual para evitar error con punto/coma
            if (!string.IsNullOrWhiteSpace(request.Pvp))
            {
                if (decimal.TryParse(request.Pvp.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var pvpValue))
                {
                    existingProduct.Pvp = pvpValue;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.PrecioMayorista))
            {
                if (decimal.TryParse(request.PrecioMayorista.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var mayoristaValue))
                {
                    existingProduct.PrecioMayorista = mayoristaValue;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.PrecioImportacion))
            {
                if (decimal.TryParse(request.PrecioImportacion.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var importacionValue))
                {
                    existingProduct.PrecioImportacion = importacionValue;
                }
            }

            if (request.Descripcion != null)
                existingProduct.Descripcion = request.Descripcion;

            if (request.UpdatedBy != null)
                existingProduct.CreatedBy = request.UpdatedBy;

            if (request.Foto != null)
            {
                var folderPath = Path.Combine("wwwroot", "uploads");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Foto.FileName)}";
                var savePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(savePath, FileMode.Create);
                await request.Foto.CopyToAsync(stream);

                existingProduct.Foto = $"/uploads/{fileName}";
            }


            await _productRepository.UpdateAsync(existingProduct);
            return existingProduct;
        }
    }
}
