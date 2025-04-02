using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AltecSystem.Application.Commands.Products;
using MediatR;
using AltecSystem.Domain.Entities;
using AltecSystem.Application.Interfaces;
using System;

namespace AltecSystem.Application.Handlers.Products
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;

        public CreateProductHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Convertir el archivo a un arreglo de bytes
            byte[] fotoBytes = null;
            if (request.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await request.Foto.CopyToAsync(memoryStream);
                    fotoBytes = memoryStream.ToArray();
                }
            }

            // Obtener la zona horaria de Ecuador (UTC-5)
            TimeZoneInfo ecuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

            // Convertir la hora UTC a la hora de Ecuador
            DateTimeOffset fechaEcuador = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ecuadorTimeZone);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Categoria = request.Categoria,
                Codigo = request.Codigo,
                Stock = request.Stock,
                Pvp = request.Pvp,
                PrecioMayorista = request.PrecioMayorista,
                PrecioImportacion = request.PrecioImportacion,
                Descripcion = request.Descripcion,
                Foto = fotoBytes,  // Almacenar el arreglo de bytes en lugar de un string
                IsActive = true,
                CreatedAt = fechaEcuador,  // Asignar la fecha con la zona horaria de Ecuador
                UpdatedAt = fechaEcuador,  // Asignar la fecha con la zona horaria de Ecuador
                CreatedBy = request.CreatedBy,
                UpdatedBy = request.CreatedBy
            };

            await _productRepository.AddAsync(product);
            return product.Id;
        }
    }
}
