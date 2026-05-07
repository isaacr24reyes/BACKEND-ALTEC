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
        public DbSet<MundialCode> MundialCodes { get; set; }
        public DbSet<MundialPronostico> MundialPronosticos { get; set; }

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

            // Eliminamos cualquier índice único en InvoiceNumber
            modelBuilder.Entity<Sale>()
                .HasIndex(s => s.InvoiceNumber)
                .IsUnique(false);

            // MundialCodes: Codigo único
            modelBuilder.Entity<MundialCode>()
                .HasIndex(m => m.Codigo)
                .IsUnique();

            modelBuilder.Entity<MundialCode>()
                .Property(m => m.Codigo)
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<MundialCode>()
                .Property(m => m.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();

            // MundialPronosticos
            modelBuilder.Entity<MundialPronostico>()
                .HasIndex(p => p.CodigoUnico)
                .IsUnique();

            modelBuilder.Entity<MundialPronostico>()
                .Property(p => p.CodigoUnico)
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<MundialPronostico>()
                .Property(p => p.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<MundialPronostico>()
                .Property(p => p.Telefono)
                .HasMaxLength(20);
        }
    }
}
