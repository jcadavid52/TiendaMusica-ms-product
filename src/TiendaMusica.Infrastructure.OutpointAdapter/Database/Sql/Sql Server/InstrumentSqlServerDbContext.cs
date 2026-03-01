using Microsoft.EntityFrameworkCore;
using TiendaMusica.Domain.Models;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.Sql_Server
{
    public class InstrumentSqlServerDbContext:DbContext
    {
        public DbSet<Instrument> Instruments { get; set; }
        public InstrumentSqlServerDbContext(DbContextOptions<InstrumentSqlServerDbContext> options) : base(options)
        {
            Instruments = Set<Instrument>();
        }
    }
}
