namespace AltecSystem.Application.DTOs.ElectronicInvoices;

public class ElectronicInvoiceResultDto
{
    public Guid Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string ClaveAcceso { get; set; } = string.Empty;
    public byte Ambiente { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Autorizada => Estado == SriInvoiceStates.Autorizado;
    public string? NumeroAutorizacion { get; set; }
    public DateTimeOffset? FechaAutorizacion { get; set; }
    public DateTime FechaEmision { get; set; }
    public string RazonSocialComprador { get; set; } = string.Empty;
    public string IdentificacionComprador { get; set; } = string.Empty;
    public decimal ImporteTotal { get; set; }
    public string? EmailComprador { get; set; }
    public bool EmailEnviado { get; set; }
    public DateTimeOffset? FechaAnulacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    public List<SriMessageDto> Mensajes { get; set; } = new();
}

public class SriMessageDto
{
    public string? Identificador { get; set; }
    public string? Mensaje { get; set; }
    public string? InformacionAdicional { get; set; }
    public string? Tipo { get; set; }
}

/// <summary>Estados de la factura en el flujo con el SRI.</summary>
public static class SriInvoiceStates
{
    public const string Firmada = "FIRMADA";
    public const string ErrorEnvio = "ERROR_ENVIO";
    public const string Recibida = "RECIBIDA";
    public const string Devuelta = "DEVUELTA";
    public const string Autorizado = "AUTORIZADO";
    public const string NoAutorizado = "NO AUTORIZADO";
    /// <summary>Anulada en SRI en línea y registrada en ALTEC.</summary>
    public const string Anulada = "ANULADA";

    /// <summary>Estados que aún pueden llegar a autorizarse (se reintentan).</summary>
    public static readonly string[] Pendientes = { Firmada, ErrorEnvio, Recibida };
}
