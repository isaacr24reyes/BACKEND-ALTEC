using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using MediatR; // Asegúrate de tener este espacio de nombres

namespace AltecSystem.Application.Commands.Products
{
    public class CreateProductCommand : IRequest<Guid> // Implementa IRequest<Guid>
    {
        [Required] // Campo obligatorio
        public string Categoria { get; set; }

        [Required] // Campo obligatorio
        public string Codigo { get; set; }

        [Required] // Campo obligatorio
        public int Stock { get; set; }
        [Required] // Campo obligatorio
        public decimal Pvp { get; set; }

        [Required] // Campo obligatorio
        public decimal PrecioMayorista { get; set; }

        [Required] // Campo obligatorio
        public decimal PrecioImportacion { get; set; }

        [Required] // Campo obligatorio
        public string Descripcion { get; set; }

        public IFormFile? Foto { get; set; } // Foto no es obligatorio

        [Required] // Campo obligatorio
        public string CreatedBy { get; set; }
    }
}