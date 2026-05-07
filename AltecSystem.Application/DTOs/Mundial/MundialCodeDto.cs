namespace AltecSystem.Application.DTOs.Mundial;

public class MundialCodeDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
