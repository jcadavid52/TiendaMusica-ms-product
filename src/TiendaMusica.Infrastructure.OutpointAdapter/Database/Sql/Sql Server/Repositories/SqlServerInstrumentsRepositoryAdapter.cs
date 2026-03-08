using TiendaMusica.Application.Ports;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.Sql_Server.Repositories
{
    internal class SqlServerInstrumentsRepositoryAdapter : IInstrumentsRepositoryPort
    {
        public Results<Instrument> Create(Instrument instrument)
        {
            throw new NotImplementedException();
        }

        public Results<IList<Instrument>> GetAll()
        {
            var result = new Results<IList<Instrument>>();

            try
            {
                var seed = new SeedDatabase();

                result.Result = seed.SeedInstrument();
            }
            catch (Exception ex)
            { 
                var error = ex.ToString();
                result.AddError(ErrorCode.SERVER_ERROR, $"Error obteniendo instrumentos-Repository {error}");
            }

            return result;
        }
    }
}
