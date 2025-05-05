using MediatR;
using Microsoft.AspNetCore.Http;

namespace AltecSystem.Application.Commands.Products
{
    public class UpdateProductCommand : IRequest<bool>
    {
        public Guid Id { get; set; }

        public string? Categoria { get; set; }
        public string? Codigo { get; set; }
        public int? Stock { get; set; }
        public decimal? Pvp { get; set; }
        public decimal? PrecioMayorista { get; set; }
        public decimal? PrecioImportacion { get; set; }
        public string? Descripcion { get; set; }
        public IFormFile? Foto { get; set; }
        public string? UpdatedBy { get; set; }
    }
}