namespace AltecSystem.Application.DTOs.ImportedQuotations;

public class ImportedQuotationSummaryDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalMonto { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
