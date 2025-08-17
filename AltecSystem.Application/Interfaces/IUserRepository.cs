using AltecSystem.Domain.Entities;
using System.Threading.Tasks;

namespace AltecSystem.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<IReadOnlyList<User>> SearchByNameAsync(
            string search,
            int skip,
            int take,
            CancellationToken ct = default);
        Task<User?> GetByNameAsync(string name, CancellationToken ct = default);
        Task UpdateAsync(User user, CancellationToken ct = default);

    }
}