using Microsoft.EntityFrameworkCore;
using Polly;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer.Repositories
{
    public class SqlServerInstrumentsRepositoryAdapter : IInstrumentsRepositoryPort
    {
        private readonly InstrumentSqlServerDbContext _context;
        private readonly IAsyncPolicy _circuitBreakerPolicy;

        public SqlServerInstrumentsRepositoryAdapter(
            InstrumentSqlServerDbContext context,
            IAsyncPolicy circuitBreakerPolicy
            )
        {
            _context = context;
            _circuitBreakerPolicy = circuitBreakerPolicy;
        }

        public async Task<Results<IList<Instrument>>> GetAllAsync(SortDirection sortDirection = SortDirection.Asc)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                IQueryable<Instrument> query = _context.Instruments;

                if (sortDirection == SortDirection.Desc)
                    query = query.OrderByDescending(i => i.CreationDateUtc);
                else
                    query = query.OrderBy(i => i.CreationDateUtc);

                var instruments = await query.ToListAsync();
                return new Results<IList<Instrument>> { Result = instruments };
            });
        }

        public async Task<Results<Instrument>> CreateAsync(Instrument instrument)
        {
            instrument.Id = Guid.NewGuid().ToString();
            instrument.CreationDateUtc = DateTime.UtcNow;

            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                await _context.Instruments.AddAsync(instrument);
                await _context.SaveChangesAsync();

                return new Results<Instrument> { Result = instrument };
            });
        }

        public async Task<Results<Instrument?>> GetByNameAsync(string name)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var instrument = await _context.Instruments
                    .FirstOrDefaultAsync(i => i.Name == name);
                return new Results<Instrument?> { Result = instrument };
            });
        }

        public async Task<Results<int>> GetStockByType(InstrumentType type)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
             {
                 var totalStock = await _context.Instruments
                     .Where(i => i.Type == type)
                     .SumAsync(i => i.Stock);
                 return new Results<int> { Result = totalStock };
             });
        }
    }
}
