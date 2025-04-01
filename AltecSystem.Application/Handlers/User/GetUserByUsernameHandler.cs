using AltecSystem.Application.Commands.User;
using AltecSystem.Application.Interfaces;
using MediatR;

namespace AltecSystem.Application.Handlers.User
{
    public class GetUserByUsernameHandler : IRequestHandler<GetUserByUsernameCommand, LoginRequestDto>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByUsernameHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<LoginRequestDto> Handle(GetUserByUsernameCommand request, CancellationToken cancellationToken)
        { 
            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if (user == null)
                return null;

            return new LoginRequestDto
            {
                Username = user.Username,
            };
        }
    }
}