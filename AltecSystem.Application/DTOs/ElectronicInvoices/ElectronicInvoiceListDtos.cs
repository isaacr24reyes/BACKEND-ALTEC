namespace AltecSystem.Application.DTOs.ElectronicInvoices;

/// <summary>Filtros del histórico de facturas.</summary>
public class ElectronicInvoiceFilter
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    /// <summary>AUTORIZADO, PENDIENTE (firmada/recibida/error de envío), ANULADA, RECHAZADA (devuelta/no autorizada).</summary>
    public string? Estado { get; set; }
    /// <summary>Texto libre: nombre, cédula/RUC o número de factura.</summary>
    public string? Buscar { get; set; }
    /// <summary>1 pruebas, 2 producción; null = el ambiente configurado.</summary>
    public byte? Ambiente { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ElectronicInvoiceListItemDto
{
    public Guid Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string ClaveAcceso { get; set; } = string.Empty;
    public byte Ambiente { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTimeOffset? FechaAutorizacion { get; set; }
    public string IdentificacionComprador { get; set; } = string.Empty;
    public string RazonSocialComprador { get; set; } = string.Empty;
    public string? EmailComprador { get; set; }
    public decimal ImporteTotal { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool EmailEnviado { get; set; }
    public string? SaleInvoiceNumber { get; set; }
    public DateTimeOffset? FechaAnulacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ElectronicInvoicePageDto
{
    public List<ElectronicInvoiceListItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    /// <summary>Facturas pendientes de autorizar en el ambiente consultado (para el aviso de la pantalla).</summary>
    public int Pendientes { get; set; }
}

public class AnularElectronicInvoiceRequest
{
    public string? Motivo { get; set; }
    /// <summary>Cancela también la venta (Sales) y devuelve el stock.</summary>
    public bool CancelarVenta { get; set; } = true;
}
