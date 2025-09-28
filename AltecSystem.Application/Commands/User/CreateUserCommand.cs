using AltecSystem.Application.DTOs.User;
using MediatR;

namespace AltecSystem.Application.Commands.User;

public class CreateUserCommand : IRequest<UserDetailsDto>
{
    public string Name { get; set; }
    public string Telefono { get; set; }
    public string Role { get; set; }

    public CreateUserCommand(string name, string telefono, string role)
    {
        Name = name;
        Telefono = telefono;
        Role = role;
    }
}
