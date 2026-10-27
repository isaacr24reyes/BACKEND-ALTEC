namespace AltecSystem.Application.DTOs.ElectronicInvoices;

/// <summary>Respuesta del servicio de Recepción del SRI.</summary>
public class SriReceptionResult
{
    public string Estado { get; set; } = string.Empty; // RECIBIDA / DEVUELTA
    public List<SriMessageDto> Mensajes { get; set; } = new();
}

/// <summary>Respuesta del servicio de Autorización del SRI.</summary>
public class SriAuthorizationResult
{
    /// <summary>AUTORIZADO, NO AUTORIZADO, o vacío si el SRI aún no la procesa.</summary>
    public string? Estado { get; set; }
    public string? NumeroAutorizacion { get; set; }
    public DateTimeOffset? FechaAutorizacion { get; set; }
    /// <summary>XML de la autorización completa (incluye el comprobante), para guardar y enviar al cliente.</summary>
    public string? XmlAutorizacion { get; set; }
    public List<SriMessageDto> Mensajes { get; set; } = new();
}
