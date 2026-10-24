using System.ComponentModel.DataAnnotations;
using AltecSystem.Application.DTOs.ImportedQuotations;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AltecSystem.Application.Commands.ImportedQuotations;

public class UploadImportedQuotationCommand : IRequest<ImportedQuotationSummaryDto>
{
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public DateTime Fecha { get; set; }

    [Required]
    public IFormFile Archivo { get; set; } = null!;

    public string? CreatedBy { get; set; }
}
