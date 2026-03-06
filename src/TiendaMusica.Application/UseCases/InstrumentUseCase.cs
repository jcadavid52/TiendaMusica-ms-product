using TiendaMusica.Application.Ports;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Application.UseCases
{
    public class InstrumentUseCase : IInstrumentUseCase
    {
        private readonly IInstrumentsRepositoryPort _instrumentsRepositoryPorts;

        public InstrumentUseCase(IInstrumentsRepositoryPort instrumentsRepositoryPorts)
        {
            _instrumentsRepositoryPorts = instrumentsRepositoryPorts;
        }
        public Results<IList<Instrument>> GetAll()
        {
            var results = new Results<IList<Instrument>>();

            try
            {
                var result = _instrumentsRepositoryPorts.GetAll();

                if (!result.IsSuccess)
                {
                    results.AddErrors(results.Errors);
                }

                results.Result = result.Result;

            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                results.AddError(ErrorCode.SERVER_ERROR, $"Error obteniendo instrumentos-UseCase {error}");
            }

            return results;
        }
    }
}
