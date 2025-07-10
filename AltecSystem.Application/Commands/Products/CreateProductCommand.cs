using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using MediatR; 

namespace AltecSystem.Application.Commands.Products
{
    public class CreateProductCommand : IRequest<Guid>
    {
        [Required]
        public string Categoria { get; set; }

        [Required] 
        public string Codigo { get; set; }

        [Required] 
        public int Stock { get; set; }
        
        [Required]
        public string Pvp { get; set; }

        [Required]
        public string PrecioMayorista { get; set; }

        [Required]
        public string PrecioImportacion { get; set; }


        [Required]
        public string Descripcion { get; set; }

        public IFormFile? Foto { get; set; }

        [Required]
        public string CreatedBy { get; set; }
    }
}