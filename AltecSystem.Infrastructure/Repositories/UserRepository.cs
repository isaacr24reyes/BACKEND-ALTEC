using AltecSystem.Application.Interfaces;
using AltecSystem.Domain.Entities;
using AltecSystem.Domain.Persistence;
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
        public async Task<IReadOnlyList<User>> SearchByNameAsync(
            string search,
            int skip,
            int take,
            CancellationToken ct = default)
        {
            var term = search.Trim().ToLower();

            return await _context.Login
                .AsNoTracking()
                .Where(u =>
                    u.Username.ToLower().Contains(term) ||
                    u.Name.ToLower().Contains(term))
                .OrderBy(u => u.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }
        public async Task<User?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            return await _context.Login
                .FirstOrDefaultAsync(u => u.Name == name, ct);
        }

        public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
            _context.Login.Update(user);
            await _context.SaveChangesAsync(ct);
        }
        public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Login
                .AsNoTracking()
                .OrderBy(u => u.Name)
                .ToListAsync(ct);
        }
        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _context.Login.AddAsync(user, ct);
            await _context.SaveChangesAsync(ct);
        }


    }
}