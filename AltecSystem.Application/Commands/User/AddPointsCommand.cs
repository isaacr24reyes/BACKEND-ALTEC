using AltecSystem.Application.DTOs.User;
using MediatR;

namespace AltecSystem.Application.Commands.User
{
    public class AddPointsCommand : IRequest<UserDetailsDto>
    {
        public string Username { get; }
        public decimal Points { get; }

        public AddPointsCommand(string username, decimal points)
        {
            Username = username;
            Points = points;
        }
    }
}