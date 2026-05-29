using Microsoft.EntityFrameworkCore;
using TiendaMusica.Domain.Models;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer
{
    public class InstrumentSqlServerDbContext : DbContext
    {
        private const string SchemaName = "ms-product";
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public InstrumentSqlServerDbContext(DbContextOptions<InstrumentSqlServerDbContext> options) : base(options)
        {
            Instruments = Set<Instrument>();
            Products = Set<Product>();
            Categories = Set<Category>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
           .UseTptMappingStrategy();

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products", SchemaName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreationDateUtc).IsRequired();
                entity.Property(e => e.Stock).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey("CategoryId")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories", SchemaName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasMany(c => c.Products)
                      .WithOne(p => p.Category)
                      .HasForeignKey("CategoryId")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasData(new Category(1, "Instrumentos", "Instrumentos musicales"));
            });

            modelBuilder.Entity<Instrument>(entity =>
            {
                entity.ToTable("Instruments", SchemaName);
                entity.Property(e => e.Type).IsRequired();
            });
        }
    }
}
