namespace AltecSystem.Application.DTOs.Auth;

public class AddPointsRequestDto
{
    public string Name { get; set; } = default!;
    public int Points { get; set; }
}