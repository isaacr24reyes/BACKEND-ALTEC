namespace AltecSystem.Application.DTOs.Mundial;

public class MundialPronosticoDto
{
    public int Id { get; set; }
    public string CodigoUnico { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Campeon { get; set; } = string.Empty;
    public string Subcampeon { get; set; } = string.Empty;
    public string TercerLugar { get; set; } = string.Empty;
    public string CuartoLugar { get; set; } = string.Empty;
    public string Goleador { get; set; } = string.Empty;
    public string ResultadoFinal { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    // Nota: Telefono intencionalmente excluido del DTO por privacidad
}
