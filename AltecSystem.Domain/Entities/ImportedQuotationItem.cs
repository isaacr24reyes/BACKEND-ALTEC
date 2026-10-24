namespace AltecSystem.Domain.Entities;

/// <summary>Fila leída del Excel importado.</summary>
public class ImportedQuotationItem
{
    public Guid Id { get; set; }
    public Guid ImportedQuotationId { get; set; }
    public int Fila { get; set; }
    public string Producto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
}
