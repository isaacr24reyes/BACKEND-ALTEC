using AltecSystem.Application.DTOs.User;
using MediatR;

namespace AltecSystem.Application.Commands.User;

public record CreateUserCommand(string Name, string Telefono, string Role) : IRequest<bool>;
