using Microsoft.EntityFrameworkCore;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.Sql_Server;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.Sql_Server.Repositories
{
    public class SqlServerInstrumentsRepositoryAdapter : IInstrumentsRepositoryPort
    {
        private readonly InstrumentSqlServerDbContext _context;

        public SqlServerInstrumentsRepositoryAdapter(InstrumentSqlServerDbContext context)
        {
            _context = context;
        }

        public async Task<Results<IList<Instrument>>> GetAllAsync()
        {
            var instruments = await _context.Instruments.ToListAsync();
            return new Results<IList<Instrument>> { Result = instruments };
        }

        public async Task<Results<Instrument>> CreateAsync(Instrument instrument)
        {
            instrument.Id = Guid.NewGuid().ToString();
            instrument.CreationDateUtc = DateTime.UtcNow;

            await _context.Instruments.AddAsync(instrument);
            await _context.SaveChangesAsync();

            return new Results<Instrument> { Result = instrument };
        }

        public async Task<Results<Instrument>> GetByNameAsync(string name)
        {
            var instrument = await _context.Instruments
                .FirstOrDefaultAsync(i => i.Name == name);

            return new Results<Instrument> { Result = instrument };
        }

        public async Task<Results<int>> GetStockByType(InstrumentType type)
        {
            var totalStock = await _context.Instruments
                .Where(i => i.Type == type)
                .SumAsync(i => i.Stock);

            return new Results<int> { Result = totalStock };
        }
    }
}
