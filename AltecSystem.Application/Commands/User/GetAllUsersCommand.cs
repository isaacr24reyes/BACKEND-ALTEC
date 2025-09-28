using AltecSystem.Application.DTOs.User;
using MediatR;

namespace AltecSystem.Application.Commands.User;

public class GetAllUsersCommand : IRequest<List<UserDetailsDto>>
{
    public GetAllUsersCommand() { }
}