namespace AltecSystem.Application.DTOs.Auth;

public class AddPointsRequestDto
{
    public string Username { get; set; } = default!;
    public decimal Points { get; set; } 
}