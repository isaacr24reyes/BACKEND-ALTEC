namespace AltecSystem.Application.DTOs.ImportedQuotations;

public class ImportedQuotationFileDto
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Archivo { get; set; } = Array.Empty<byte>();
}
