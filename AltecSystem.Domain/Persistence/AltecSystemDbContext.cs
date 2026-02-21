using AltecSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AltecSystem.Domain.Persistence
{
    public class AltecSystemDbContext : DbContext
    {
        public AltecSystemDbContext(DbContextOptions<AltecSystemDbContext> options) : base(options) { }

        public DbSet<User> Login { get; set; }
        public DbSet<Product> Productos { get; set; }
        public DbSet<Sale> Sales { get; set; } // Agregado para incluir la entidad Sale
        public DbSet<QuotationDetail> QuotationDetails { get; set; } // Agregado para incluir la entidad QuotationDetail

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Asegúrate de llamar al método base

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            
            modelBuilder.Entity<Product>()
                .Property(p => p.Foto)
                .HasColumnType("NVARCHAR(4000)"); // Cambiado de varbinary(max) a NVARCHAR(4000) para URL

            modelBuilder.Entity<Sale>()
                .Property(s => s.ProductId) // Corregido el nombre de la propiedad
                .HasColumnName("ProductID")
                .IsRequired();

            // Configuración adicional para evitar errores
            modelBuilder.Entity<Sale>()
                .Property(s => s.Profit)
                .HasColumnType("decimal(18, 2)");
        }
    }
}
