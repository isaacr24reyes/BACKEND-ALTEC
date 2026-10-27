namespace AltecSystem.Domain.Entities;

/// <summary>Datos del emisor y secuenciales para la facturación electrónica (una sola fila, Id = 1).</summary>
public class SriSettings
{
    public int Id { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string DirMatriz { get; set; } = string.Empty;
    public string DirEstablecimiento { get; set; } = string.Empty;
    public string ObligadoContabilidad { get; set; } = "NO";
    public string? ContribuyenteRimpe { get; set; }
    public string? ContribuyenteEspecial { get; set; }
    public string? AgenteRetencion { get; set; }
    public string Estab { get; set; } = "001";
    public string PtoEmi { get; set; } = "001";
    public int SiguienteSecuencialPruebas { get; set; }
    public int SiguienteSecuencialProduccion { get; set; }
    public string CodigoPorcentajeIva { get; set; } = "0";
    public decimal TarifaIva { get; set; }
    public string? EmailCopia { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
