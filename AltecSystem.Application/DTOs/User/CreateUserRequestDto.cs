namespace AltecSystem.Application.DTOs.User;

public class CreateUserRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Role { get; set; } = "Cliente";
}