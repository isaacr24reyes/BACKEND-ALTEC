namespace AltecSystem.Domain.Entities;

public class MundialCode
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
