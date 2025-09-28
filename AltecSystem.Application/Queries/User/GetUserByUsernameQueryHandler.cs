using AltecSystem.Application.DTOs.User;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.User;
using MediatR;

namespace AltecSystem.Application.Handlers.User;

public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, UserDetailsDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByUsernameQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDetailsDto> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null)
            return null;

        return new UserDetailsDto
        {
            Username = user.Username,
            Name = user.Name,
            Role = user.Role,
            Altec_Points = user.Altec_Points ?? 0m
        };

    }
}