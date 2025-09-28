using AltecSystem.Application.Commands.User;
using AltecSystem.Application.DTOs.User;
using AltecSystem.Application.Interfaces;
using MediatR;

namespace AltecSystem.Application.Handlers.User
{
    public class AddPointsHandler : IRequestHandler<AddPointsCommand, UserDetailsDto>
    {
        private readonly IUserRepository _userRepository;

        public AddPointsHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDetailsDto> Handle(AddPointsCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByNameAsync(request.Name, cancellationToken);
            if (user == null)
                throw new KeyNotFoundException($"Usuario con nombre '{request.Name}' no encontrado.");

            user.Altec_Points = (user.Altec_Points ?? 0) + request.Points; // ahora maneja decimales

            await _userRepository.UpdateAsync(user, cancellationToken);

            return new UserDetailsDto
            {
                Username = user.Username,
                Name = user.Name,
                Role = user.Role,
                Altec_Points = user.Altec_Points ?? 0
            };
        }

    }
}