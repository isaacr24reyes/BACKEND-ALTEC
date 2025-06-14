using AltecSystem.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AltecSystem.Application.Commands.Products
{
    public class UpdateProductCommand : IRequest<Product> // antes: IRequest<bool>
    {
        public Guid Id { get; set; }

        public string? Categoria { get; set; }
        public string? Codigo { get; set; }
        public int? Stock { get; set; }
        public string? Pvp { get; set; }
        public string? PrecioMayorista { get; set; }
        public string? PrecioImportacion { get; set; }

        public string? Descripcion { get; set; }
        public IFormFile? Foto { get; set; }
        public string? UpdatedBy { get; set; }
    }
}