using AltecSystem.Application.Commands.User;
using AltecSystem.Application.Interfaces;
using MediatR;

namespace AltecSystem.Application.Handlers.User
{
    public class CreateUserCommandHandler(IUserRepository userRepository)
        : IRequestHandler<CreateUserCommand, bool>
    {
        public async Task<bool> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Username = primer nombre en minúsculas
            var username = request.Name.Split(' ')[0].ToLower();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Telefono);

            var user = new Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Username = username,
                PasswordHash = passwordHash,
                Role = request.Role, // Cliente o Distribuidor
                Altec_Points = 0,
                Telefono = request.Telefono
            };

            await userRepository.AddAsync(user, cancellationToken);
            return true;
        }
    }
}