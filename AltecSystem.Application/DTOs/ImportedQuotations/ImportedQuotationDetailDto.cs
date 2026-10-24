namespace AltecSystem.Application.DTOs.ImportedQuotations;

public class ImportedQuotationDetailDto : ImportedQuotationSummaryDto
{
    public List<ImportedQuotationItemDto> Items { get; set; } = new();
}

public class ImportedQuotationItemDto
{
    public int Fila { get; set; }
    public string Producto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
}
