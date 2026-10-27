namespace AltecSystem.Application.DTOs.ElectronicInvoices;

/// <summary>Datos que envía el POS para emitir una factura electrónica.</summary>
public class CreateElectronicInvoiceRequest
{
    /// <summary>Número de venta interno (Sales.InvoiceNumber) para enlazar la factura con la venta.</summary>
    public string? SaleInvoiceNumber { get; set; }

    /// <summary>Código SRI de forma de pago: 01 efectivo, 20 transferencia / otros con sistema financiero.</summary>
    public string FormaPago { get; set; } = "01";

    public InvoiceCustomerDto Cliente { get; set; } = new();
    public List<InvoiceItemRequestDto> Items { get; set; } = new();
    public string? CreatedBy { get; set; }
}

public class InvoiceCustomerDto
{
    /// <summary>Opcional: si no se envía se deduce (13 dígitos terminados en 001 = RUC, 10 dígitos = cédula, otro = pasaporte).</summary>
    public string? TipoIdentificacion { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}

public class InvoiceItemRequestDto
{
    public Guid? ProductId { get; set; }
    public string? Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
}
