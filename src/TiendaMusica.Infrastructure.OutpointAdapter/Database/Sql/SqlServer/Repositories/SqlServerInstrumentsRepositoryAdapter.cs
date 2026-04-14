using Microsoft.EntityFrameworkCore;
using Polly;
using System.Linq.Expressions;
using TiendaMusica.Domain.Dtos;
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

        public async Task<Results<IList<Instrument>>> GetAllAsync(
            SortDirection sortDirection = SortDirection.Desc,
            Expression<Func<Instrument, bool>>[]? filters = null,
            int? skip = null,
            int? take = null
            )
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                IQueryable<Instrument> query = _context.Instruments.AsNoTracking();

                if (sortDirection == SortDirection.Desc)
                    query = query.OrderByDescending(i => i.CreationDateUtc);
                else
                    query = query.OrderBy(i => i.CreationDateUtc);

                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = query.Where(filter);
                    }
                }

                if (skip.HasValue && skip.Value > 0)
                {
                    query = query.Skip(skip.Value);
                }

                if (take.HasValue && take.Value > 0)
                {
                    query = query.Take(take.Value);
                }

                var instruments = await query.ToListAsync();
                return new Results<IList<Instrument>>
                {
                    Result = instruments,
                };
            });
        }

        public async Task<Results<Instrument?>> GetByNameAsync(string name)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var instrument = await _context.Instruments.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Name == name);
                return new Results<Instrument?> { Result = instrument };
            });
        }

        public async Task<Results<Instrument?>> GetByIdAsync(string id)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var instrument = await _context.Instruments.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == id);
                return new Results<Instrument?> { Result = instrument };
            });
        }

        public async Task<Results<IList<Instrument>>> GetByIdsAsync(IList<string> instrumentIds)
        {
            var results = new Results<IList<Instrument>>();

            var toDelete = await _context.Instruments
                .Where(i => instrumentIds.Contains(i.Id))
                .ToListAsync();

            results.Result = toDelete;
            return results;
        }

        public async Task<Results<int>> GetStockByType(InstrumentType type)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
             {
                 var totalStock = await _context.Instruments.AsNoTracking()
                     .Where(i => i.Type == type)
                     .SumAsync(i => i.Stock);
                 return new Results<int> { Result = totalStock };
             });
        }

        public async Task<Results<IList<InstrumentStockSummary>>> GetStockSummaryByInstrumentTypesAsync(IList<string> instrumentIds)
        {
            var results = new Results<IList<InstrumentStockSummary>>();

            var resultado = await _context.Instruments
                .Where(i => _context.Instruments
                    .Where(sub => instrumentIds.Contains(sub.Id))
                    .Select(sub => sub.Type)
                    .Contains(i.Type))
                .GroupBy(i => i.Type)
                .Select(g => new InstrumentStockSummary(
                    g.Key,
                    g.Sum(i => i.Stock)
                ))
                .ToListAsync();

            results.Result = resultado;
            return results;
        }

        public async Task<Results<Instrument>> CreateAsync(Instrument instrument)
        {
            await _context.Instruments.AddAsync(instrument);

            return new Results<Instrument> { Result = instrument };
        }

        public void Update(Instrument instrument)
        {
            _context.Instruments.Update(instrument);
        }

        public void DeleteMultipleAsync(IList<Instrument> instruments)
        {
            _context.Instruments.RemoveRange(instruments);
        }


    }
}
