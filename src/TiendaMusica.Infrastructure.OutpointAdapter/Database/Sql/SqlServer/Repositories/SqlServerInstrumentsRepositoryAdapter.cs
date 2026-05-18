using Microsoft.EntityFrameworkCore;
using Polly;
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

        public async Task<Results<IList<Instrument>>> GetAllAsync(InstrumentGetAllQueryParametersDto? queryParameters)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var results = new Results<IList<Instrument>>();
                IQueryable<Instrument> query = _context.Instruments.AsNoTracking();

                if (queryParameters == null)
                {
                    results.Result = await query.ToListAsync();
                    return results;
                }

                if (!string.IsNullOrWhiteSpace(queryParameters.Search))
                {
                    string search = queryParameters.Search.ToLower();

                    query = query.Where(p => p.Name.ToLower().Contains(search)
                                          || p.Description.ToLower().Contains(search));
                }

                if (!string.IsNullOrWhiteSpace(queryParameters.OrderBy))
                {
                    query = queryParameters.OrderBy.ToLower() switch
                    {
                        "name" => queryParameters.SortDirection == SortDirection.Asc ? query.OrderBy(i => i.Name) : query.OrderByDescending(i => i.Name),
                        "price" => queryParameters.SortDirection == SortDirection.Asc ? query.OrderBy(i => i.Price) : query.OrderByDescending(i => i.Price),
                        "stock" => queryParameters.SortDirection == SortDirection.Asc ? query.OrderBy(i => i.Stock) : query.OrderByDescending(i => i.Stock),
                        "type" => queryParameters.SortDirection == SortDirection.Asc ? query.OrderBy(i => i.Type) : query.OrderByDescending(i => i.Type),
                        "creationdateutc" => queryParameters.SortDirection == SortDirection.Asc ? query.OrderBy(i => i.CreationDateUtc) : query.OrderByDescending(i => i.CreationDateUtc),
                        _ => query.OrderBy(p => p.Id)
                    };
                }

                if (queryParameters.PageNumber > 0 && queryParameters.PageSize > 0)
                {
                    query = query.Skip((queryParameters.PageNumber.Value - 1) * queryParameters.PageSize.Value)
                                 .Take(queryParameters.PageSize.Value);
                }

                results.Result = await query.ToListAsync();
                return results;
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

        public async Task<Results<Instrument?>> GetByNameAsync(string name)
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var instrument = await _context.Instruments.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Name == name);
                return new Results<Instrument?> { Result = instrument };
            });
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

        public void DeleteMultiple(IList<Instrument> instruments)
        {
            _context.Instruments.RemoveRange(instruments);
        }
    }
}
