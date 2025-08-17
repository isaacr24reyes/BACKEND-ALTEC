using AltecSystem.Application.DTOs.User;
using MediatR;

namespace AltecSystem.Application.Commands.User
{
    public class AddPointsCommand : IRequest<UserDetailsDto>
    {
        public string Name { get; }
        public int Points { get; }

        public AddPointsCommand(string name, int points)
        {
            Name = name;
            Points = points;
        }
    }
}