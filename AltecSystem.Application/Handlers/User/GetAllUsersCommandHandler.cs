using AltecSystem.Application.Commands.User;
using AltecSystem.Application.DTOs.User;
using AltecSystem.Application.Interfaces;
using MediatR;

namespace AltecSystem.Application.Handlers.User
{
    public class GetAllUsersCommandHandler(IUserRepository userRepository)
        : IRequestHandler<GetAllUsersCommand, List<UserDetailsDto>>
    {
        public async Task<List<UserDetailsDto>> Handle(GetAllUsersCommand request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllAsync(cancellationToken);

            return users.Select(u => new UserDetailsDto
            {
                Username = u.Username,
                Name = u.Name,
                Role = u.Role,
                Altec_Points = u.Altec_Points,
                Telefono = u.Telefono
            }).ToList();
        }
    }
}