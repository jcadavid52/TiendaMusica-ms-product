using Microsoft.EntityFrameworkCore;
using TiendaMusica.Domain.Models;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer
{
    public class InstrumentSqlServerDbContext:DbContext
    {
        private const string SchemaName = "ms-instruments";
        public DbSet<Instrument> Instruments { get; set; }
        public InstrumentSqlServerDbContext(DbContextOptions<InstrumentSqlServerDbContext> options) : base(options)
        {
            Instruments = Set<Instrument>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Instrument>(entity =>
            {
                entity.ToTable("Instruments",SchemaName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreationDateUtc).IsRequired();
                entity.Property(e => e.Stock).IsRequired();
                entity.Property(e => e.Type).IsRequired();
            });
        }
    }
}
