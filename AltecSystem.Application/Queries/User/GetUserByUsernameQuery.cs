using AltecSystem.Application.DTOs.User;
using MediatR;

namespace AltecSystem.Application.Queries.User;

public class GetUserByUsernameQuery : IRequest<UserDetailsDto>
{
    public string Username { get; set; }

    public GetUserByUsernameQuery(string username)
    {
        Username = username;
    }
}