using MediatR;

namespace AltecSystem.Application.Commands.User;

public class GetUserByUsernameCommand : IRequest<LoginRequestDto>
{
    public string Username { get; set; }

    public GetUserByUsernameCommand(string username)
    {
        Username = username;
    }
}