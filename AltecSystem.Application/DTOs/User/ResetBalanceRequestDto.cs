namespace AltecSystem.Application.DTOs.User;

public class ResetBalanceRequestDto
{
    public string Username { get; set; } = default!;
    public decimal NewBalance { get; set; }
}
