using AltecSystem.Application.Commands.Products;
using MediatR;
using AltecSystem.Domain.Entities;
using AltecSystem.Application.Interfaces;
using Microsoft.Extensions.Hosting;


namespace AltecSystem.Application.Handlers.Products
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;
        private readonly IHostEnvironment _env;

        public CreateProductHandler(IProductRepository productRepository, IHostEnvironment env)
        {
            _productRepository = productRepository;
            _env = env;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {

            string fotoUrl = null;
            if (request.Foto != null)
            {
                // Generar un nombre único para la imagen
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.Foto.FileName);
                var filePath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", fileName); // Uso ContentRootPath
                Console.WriteLine("Ruta de la imagen: " + fotoUrl);  // Aquí se imprimirá el valor de fotoUrl

                // Guardar la imagen en la carpeta wwwroot/uploads
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Foto.CopyToAsync(stream);
                }

                // Asignar la ruta de la imagen
                fotoUrl = "/uploads/" + fileName;  // Ruta relativa
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
                Foto = fotoUrl ?? "NOT-IMAGE",
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
