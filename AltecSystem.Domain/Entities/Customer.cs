namespace AltecSystem.Domain.Entities;

/// <summary>Cliente al que se emite factura electrónica.</summary>
public class Customer
{
    public Guid Id { get; set; }
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
