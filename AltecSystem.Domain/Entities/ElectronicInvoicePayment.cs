namespace AltecSystem.Domain.Entities;

public class ElectronicInvoicePayment
{
    public Guid Id { get; set; }
    public Guid ElectronicInvoiceId { get; set; }
    public string FormaPago { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Plazo { get; set; }
    public string UnidadTiempo { get; set; } = "Dias";
}
