using AltecSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AltecSystem.Infrastructure.Persistence
{
    public class AltecSystemDbContext : DbContext
    {
        public AltecSystemDbContext(DbContextOptions<AltecSystemDbContext> options) : base(options) { }

        public DbSet<User> Login { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}