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
        public async Task<Results<IList<Instrument>>> GetAllAsync()
        {
            var results = new Results<IList<Instrument>>();

            try
            {
                var result = await _instrumentsRepositoryPorts.GetAllAsync();

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
        public async Task<Results<Instrument>> CreateAsync(Instrument instrument)
        {
            var results = new Results<Instrument>();

            try
            {
                var result = await _instrumentsRepositoryPorts.CreateAsync(instrument);

                if (!result.IsSuccess)
                {
                    results.AddErrors(result.Errors);
                }

                results.Result = result.Result;
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                results.AddError(ErrorCode.SERVER_ERROR, $"Error creando instrumentos-UseCase {error}");
            }

            return results;
        }
    }
}
