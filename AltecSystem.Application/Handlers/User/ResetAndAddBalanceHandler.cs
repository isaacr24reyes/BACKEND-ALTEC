using AltecSystem.Application.Commands.User;
using AltecSystem.Application.DTOs.User;
using AltecSystem.Application.Interfaces;
using MediatR;

namespace AltecSystem.Application.Handlers.User
{
    public class ResetAndAddBalanceHandler : IRequestHandler<ResetAndAddBalanceCommand, UserDetailsDto>
    {
        private readonly IUserRepository _userRepository;

        public ResetAndAddBalanceHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDetailsDto> Handle(ResetAndAddBalanceCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
                throw new KeyNotFoundException($"Usuario con username '{request.Username}' no encontrado.");

            // 1️⃣ Resetear balance
            user.Altec_Points = 0;

            // 2️⃣ Acreditar nuevo balance
            user.Altec_Points += request.NewBalance;

            await _userRepository.UpdateAsync(user, cancellationToken);

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