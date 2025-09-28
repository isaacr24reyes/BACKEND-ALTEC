using AltecSystem.Application.DTOs.User;
using MediatR;

namespace AltecSystem.Application.Commands.User
{
    public class AddPointsCommand : IRequest<UserDetailsDto>
    {
        public string Name { get; }
        public decimal Points { get; }

        public AddPointsCommand(string name, decimal points)
        {
            Name = name;
            Points = points;
        }
    }
}