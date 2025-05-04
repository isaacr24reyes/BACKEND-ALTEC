using AltecSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AltecSystem.Infrastructure.Persistence
{
    public class AltecSystemDbContext : DbContext
    {
        public AltecSystemDbContext(DbContextOptions<AltecSystemDbContext> options) : base(options) { }

        public DbSet<User> Login { get; set; }
        public DbSet<Product> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            
            modelBuilder.Entity<Product>()
                .Property(p => p.Foto)
                .HasColumnType("NVARCHAR(4000)"); // Cambiado de varbinary(max) a NVARCHAR(4000) para URL

            // Otras configuraciones si las necesitas
        }
    }
}