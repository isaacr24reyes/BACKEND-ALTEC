using AltecSystem.Domain.Entities;
using System.Threading.Tasks;

namespace AltecSystem.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
    }
}