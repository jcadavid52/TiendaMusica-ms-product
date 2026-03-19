using Microsoft.EntityFrameworkCore;
using TiendaMusica.Application.Ports;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
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
            var result = new Results<IList<Instrument>>();

            try
            {
                var instruments = await _context.Instruments.ToListAsync();
                result.Result = instruments;
            }
            catch (Exception ex)
            { 
                result.AddError(ErrorCode.SERVER_ERROR, $"Error obteniendo instrumentos-Repository {ex.Message}");
            }

            return result;
        }

        public async Task<Results<Instrument>> CreateAsync(Instrument instrument)
        {
            var result = new Results<Instrument>();

            try
            {
                instrument.Id = Guid.NewGuid().ToString();
                instrument.CreationDateUtc = DateTime.UtcNow;

                await _context.Instruments.AddAsync(instrument);
                await _context.SaveChangesAsync();

                result.Result = instrument;
            }
            catch (Exception ex)
            {
                result.AddError(ErrorCode.SERVER_ERROR, $"Error creando instrumento-SQL-Repository: {ex.Message}");
            }

            return result;
        }

        public async Task<Results<Instrument>> GetByNameAsync(string name)
        {
            var result = new Results<Instrument>();

            try
            {
                var instrument = await _context.Instruments
                    .FirstOrDefaultAsync(i => i.Name == name);

                result.Result = instrument;
            }
            catch (Exception ex)
            {
                result.AddError(ErrorCode.SERVER_ERROR, $"Error buscando instrumento-SQL-Repository: {ex.Message}");
            }

            return result;
        }

        public async Task<Results<int>> GetStockByType(InstrumentType type)
        {
            var result = new Results<int>();

            try
            {
                var totalStock = await _context.Instruments
                    .Where(i => i.Type == type)
                    .SumAsync(i => i.Stock);

                result.Result = totalStock;
            }
            catch (Exception ex)
            {
                result.AddError(ErrorCode.SERVER_ERROR, $"Error obteniendo stock-SQL-Repository: {ex.Message}");
            }

            return result;
        }
    }
}
