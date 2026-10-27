namespace AltecSystem.Domain.Entities;

/// <summary>Factura electrónica emitida al SRI.</summary>
public class ElectronicInvoice
{
    public Guid Id { get; set; }
    public byte Ambiente { get; set; }
    public string Estab { get; set; } = string.Empty;
    public string PtoEmi { get; set; } = string.Empty;
    public int Secuencial { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string ClaveAcceso { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public Guid CustomerId { get; set; }
    public string TipoIdentificacionComprador { get; set; } = string.Empty;
    public string IdentificacionComprador { get; set; } = string.Empty;
    public string RazonSocialComprador { get; set; } = string.Empty;
    public string? DireccionComprador { get; set; }
    public string? EmailComprador { get; set; }
    public string? TelefonoComprador { get; set; }
    public decimal TotalSinImpuestos { get; set; }
    public decimal TotalDescuento { get; set; }
    public decimal TotalIva { get; set; }
    public decimal ImporteTotal { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? MensajesSri { get; set; }
    public string? NumeroAutorizacion { get; set; }
    public DateTimeOffset? FechaAutorizacion { get; set; }
    public string XmlFirmado { get; set; } = string.Empty;
    public string? XmlAutorizado { get; set; }
    public string? SaleInvoiceNumber { get; set; }
    public bool EmailEnviado { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public List<ElectronicInvoiceItem> Items { get; set; } = new();
    public List<ElectronicInvoicePayment> Payments { get; set; } = new();
}
