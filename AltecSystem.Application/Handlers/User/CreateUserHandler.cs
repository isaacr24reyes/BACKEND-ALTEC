using AltecSystem.Application.Commands.User;
using AltecSystem.Application.DTOs.User;
using AltecSystem.Application.Interfaces;
using MediatR;

namespace AltecSystem.Application.Handlers.User
{
    public class CreateUserCommandHandler(IUserRepository userRepository)
        : IRequestHandler<CreateUserCommand, UserDetailsDto>
    {
        public async Task<UserDetailsDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Username único
            var baseUsername = request.Name.Split(' ')[0].ToLower();
            var username = baseUsername;
            int counter = 1;

            while (await userRepository.GetByUsernameAsync(username) != null)
            {
                username = $"{baseUsername}{counter}";
                counter++;
            }

            // Hash contraseña = teléfono
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Telefono);

            var user = new Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Username = username,
                PasswordHash = passwordHash,
                Role = request.Role,
                Altec_Points = 0,
                Telefono = request.Telefono
            };

            await userRepository.AddAsync(user, cancellationToken);

            return new UserDetailsDto
            {
                Username = user.Username,
                Name = user.Name,
                Role = user.Role,
                Altec_Points = user.Altec_Points ?? 0,
                Telefono = user.Telefono
            };
        }
    }
}