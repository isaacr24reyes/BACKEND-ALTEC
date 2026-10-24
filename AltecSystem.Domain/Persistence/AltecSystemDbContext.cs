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
        public DbSet<ImportedQuotation> ImportedQuotations { get; set; }
        public DbSet<ImportedQuotationItem> ImportedQuotationItems { get; set; }

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

            // Historial de cotizaciones importadas (script: Database/001_historial_cotizaciones.sql)
            modelBuilder.Entity<ImportedQuotation>(e =>
            {
                e.Property(q => q.Nombre).HasMaxLength(200).IsRequired();
                e.Property(q => q.Fecha).HasColumnType("date");
                e.Property(q => q.NombreArchivo).HasMaxLength(260).IsRequired();
                e.Property(q => q.ContentType).HasMaxLength(150).IsRequired();
                e.Property(q => q.Archivo).HasColumnType("varbinary(max)").IsRequired();
                e.Property(q => q.TotalMonto).HasColumnType("decimal(18,4)");
                e.Property(q => q.CreatedBy).HasMaxLength(100).IsRequired();
                e.HasIndex(q => q.Fecha);
                e.HasMany(q => q.Items)
                    .WithOne()
                    .HasForeignKey(i => i.ImportedQuotationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ImportedQuotationItem>(e =>
            {
                e.Property(i => i.Producto).HasMaxLength(500).IsRequired();
                e.Property(i => i.Cantidad).HasColumnType("decimal(18,4)");
                e.Property(i => i.PrecioUnitario).HasColumnType("decimal(18,6)");
                e.Property(i => i.Total).HasColumnType("decimal(18,6)");
            });
        }
    }
}
