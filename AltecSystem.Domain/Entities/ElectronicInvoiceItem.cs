namespace AltecSystem.Domain.Entities;

public class ElectronicInvoiceItem
{
    public Guid Id { get; set; }
    public Guid ElectronicInvoiceId { get; set; }
    public int Linea { get; set; }
    public Guid? ProductId { get; set; }
    public string CodigoPrincipal { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal PrecioTotalSinImpuesto { get; set; }
    public string CodigoPorcentajeIva { get; set; } = "0";
    public decimal TarifaIva { get; set; }
    public decimal ValorIva { get; set; }
}
