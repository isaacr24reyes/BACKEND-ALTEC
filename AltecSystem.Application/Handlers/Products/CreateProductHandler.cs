using AltecSystem.Application.Commands.Products;
using MediatR;
using AltecSystem.Domain.Entities;
using AltecSystem.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Globalization;

namespace AltecSystem.Application.Handlers.Products
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;
        private readonly CloudinaryService _cloudinaryService;

        public CreateProductHandler(IProductRepository productRepository, CloudinaryService cloudinaryService)
        {
            _productRepository = productRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            string fotoUrl = "NOT-IMAGE";

            if (request.Foto != null && request.Foto.Length > 0)
            {
                fotoUrl = await _cloudinaryService.UploadImageAsync(request.Foto, "imagenes-ALTEC");
            }
            
            decimal pvp = decimal.Parse(request.Pvp.Replace(",", "."), CultureInfo.InvariantCulture);
            decimal precioMayorista = decimal.Parse(request.PrecioMayorista.Replace(",", "."), CultureInfo.InvariantCulture);
            decimal precioImportacion = decimal.Parse(request.PrecioImportacion.Replace(",", "."), CultureInfo.InvariantCulture);

            TimeZoneInfo ecuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            DateTimeOffset fechaEcuador = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ecuadorTimeZone);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Categoria = request.Categoria,
                Codigo = request.Codigo,
                Stock = request.Stock,
                Pvp = pvp,
                PrecioMayorista = precioMayorista,
                PrecioImportacion = precioImportacion,
                Descripcion = request.Descripcion,
                Foto = fotoUrl,
                IsActive = true,
                CreatedAt = fechaEcuador,
                UpdatedAt = fechaEcuador,
                CreatedBy = request.CreatedBy,
                UpdatedBy = request.CreatedBy
            };

            await _productRepository.AddAsync(product);
            return product.Id;
        }
    }
}
