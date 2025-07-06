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
        private readonly CloudinaryService _cloudinaryService;

        public UpdateProductHandler(IProductRepository productRepository, CloudinaryService cloudinaryService)
        {
            _productRepository = productRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Product> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existingProduct = await _productRepository.GetByIdAsync(request.Id);
            if (existingProduct == null)
                return null!;

            if (request.Categoria != null)
                existingProduct.Categoria = request.Categoria;

            if (request.Codigo != null)
                existingProduct.Codigo = request.Codigo;

            if (request.Stock.HasValue)
                existingProduct.Stock = request.Stock.Value;

            if (!string.IsNullOrWhiteSpace(request.Pvp) &&
                decimal.TryParse(request.Pvp.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var pvpValue))
            {
                existingProduct.Pvp = pvpValue;
            }

            if (!string.IsNullOrWhiteSpace(request.PrecioMayorista) &&
                decimal.TryParse(request.PrecioMayorista.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var mayoristaValue))
            {
                existingProduct.PrecioMayorista = mayoristaValue;
            }

            if (!string.IsNullOrWhiteSpace(request.PrecioImportacion) &&
                decimal.TryParse(request.PrecioImportacion.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var importacionValue))
            {
                existingProduct.PrecioImportacion = importacionValue;
            }

            if (request.Descripcion != null)
                existingProduct.Descripcion = request.Descripcion;

            if (request.UpdatedBy != null)
                existingProduct.UpdatedBy = request.UpdatedBy;

            // ✅ Subida a Cloudinary
            if (request.Foto != null)
            {
                var cloudUrl = await _cloudinaryService.UploadImageAsync(request.Foto, "imagenes-ALTEC");
                if (!string.IsNullOrEmpty(cloudUrl))
                {
                    existingProduct.Foto = cloudUrl;
                }
            }

            await _productRepository.UpdateAsync(existingProduct);
            return existingProduct;
        }
    }
}
