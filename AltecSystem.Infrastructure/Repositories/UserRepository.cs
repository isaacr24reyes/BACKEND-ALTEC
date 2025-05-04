using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace AltecSystem.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AltecSystemDbContext _context;

        public UserRepository(AltecSystemDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Login.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}