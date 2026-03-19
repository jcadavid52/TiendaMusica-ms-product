using TiendaMusica.Application.Dtos;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Domain.Services;

namespace TiendaMusica.Application.UseCases.Instruments
{
    public class InstrumentUseCase : IInstrumentUseCase
    {
        private readonly IInstrumentsRepositoryPort _instrumentsRepositoryPorts;
        private readonly IInstrumentCreateValidationService _instrumentCreateValidationService;
        public InstrumentUseCase(
            IInstrumentsRepositoryPort instrumentsRepositoryPorts,
            IInstrumentCreateValidationService instrumentCreateValidationService
            )
        {
            _instrumentsRepositoryPorts = instrumentsRepositoryPorts;
            _instrumentCreateValidationService = instrumentCreateValidationService;
        }
        public async Task<Results<IList<Instrument>>> GetAllAsync()
        {
            var resultInstruments = await _instrumentsRepositoryPorts.GetAllAsync();

            if (resultInstruments.HasErrors) 
                return new Results<IList<Instrument>>().AddErrors(resultInstruments.Errors);

            return new Results<IList<Instrument>> { Result = resultInstruments.Result };
        }
        public async Task<Results<Instrument>> CreateAsync(CreateInstrumentCommand instrumentCommand)
        {
            var results = new Results<Instrument>();

            try
            {
                var currentLimitStockResult = await _instrumentsRepositoryPorts.GetStockByType(instrumentCommand.Type);
                if (currentLimitStockResult.HasErrors) return results.AddErrors(currentLimitStockResult.Errors);

                var validation = _instrumentCreateValidationService.ValidateLimitStockByType(instrumentCommand.Stock,currentLimitStockResult.Result, instrumentCommand.Type);
                if (!validation.IsSuccess && validation.HasErrors) return results.AddErrors(validation.Errors);

                var existing = await _instrumentsRepositoryPorts.GetByNameAsync(instrumentCommand.Name);
                if (existing.HasErrors) return results.AddErrors(existing.Errors);

                if (existing.Result != null)
                    return results.AddError(ErrorCode.CONFLICT_ERROR, $"Ya existe: '{instrumentCommand.Name}'");

                var instrument = Instrument.Create(
                    instrumentCommand.Name,
                    instrumentCommand.Description,
                    instrumentCommand.Type,
                    instrumentCommand.Price,
                    instrumentCommand.Stock
                    );

                if(instrument.HasErrors) return results.AddErrors(instrument.Errors);

                var resultCreate = await _instrumentsRepositoryPorts.CreateAsync(instrument.Result);
                return resultCreate.HasErrors ? results.AddErrors(resultCreate.Errors) : resultCreate;
            }
            catch (ArgumentException ex)
            {
                return results.AddError(ErrorCode.VALIDATION_ERROR, $"Error Domain: {ex.Message}");
            }
        }
    }
}
