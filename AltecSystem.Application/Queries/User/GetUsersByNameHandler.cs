using AltecSystem.Application.DTOs.User;
using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.User;
using MediatR;

namespace AltecSystem.Application.Handlers.User
{
    public class GetUsersByNameHandler : IRequestHandler<GetUsersByNameQuery, IReadOnlyList<UserDetailsDto>>
    {
        private readonly IUserRepository _userRepository;

        public GetUsersByNameHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IReadOnlyList<UserDetailsDto>> Handle(GetUsersByNameQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.SearchByNameAsync(
                request.Search,
                (request.Page - 1) * request.PageSize,
                request.PageSize,
                cancellationToken);

            // Map a DTO (ajusta campos según tu DTO real)
            var list = users.Select(u => new UserDetailsDto
                {
                    Username = u.Username,
                    Name = u.Name,
                    Role = u.Role,
                    Altec_Points = u.Altec_Points ?? 0

                })
                .ToList()
                .AsReadOnly();

            return list;
        }
    }
}