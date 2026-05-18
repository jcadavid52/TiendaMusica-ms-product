using Microsoft.Extensions.Logging;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Domain.Services;

namespace TiendaMusica.Application.UseCases.Validators.Instruments
{
    public class InstrumentCreateValidator : IInstrumentValidator<InstrumentCreateCommand, bool>
    {
        private readonly IInstrumentsRepositoryPort _repository;
        private readonly IInstrumentValidationService _validationService;
        private readonly ILogger<InstrumentCreateValidator> _logger;

        public InstrumentCreateValidator(
            IInstrumentsRepositoryPort repository,
            IInstrumentValidationService validationService,
            ILogger<InstrumentCreateValidator> logger
            )
        {
            _repository = repository;
            _validationService = validationService;
            _logger = logger;
        }

        public async Task<Results<bool>> ValidateAsync(InstrumentCreateCommand command)
        {
            var results = new Results<bool>();

            _logger.LogInformation("Iniciando validación para creación de instrumento");

            var stockResult = await _repository.GetStockByType(command.Type);
            if (stockResult.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores llamando al respositorio sql server para obtener el stock actual por tipo:{Errors}", stockResult.Errors);
                results.Result = false;
                return results.AddErrors(stockResult.Errors);
            }

            var stockValidation = _validationService.ValidateLimitStockByType(command.Stock, stockResult.Result, command.Type);
            if (!stockValidation.IsSuccess && stockValidation.HasErrors)
            {
                _logger.LogWarning("Error validación de stock por tipo '{Type}':{Errors}", command.Type, stockValidation.Errors);
                results.Result = false;
                return results.AddErrors(stockValidation.Errors);
            }

            var nameConflict = await _repository.GetByNameAsync(command.Name);

            if (nameConflict.HasErrors)
            {
                _logger.LogWarning("Error al buscar conflicto de nombre: {Errors}", nameConflict.Errors);
                results.Result = false;
                return results.AddErrors(nameConflict.Errors);
            }

            if (nameConflict.Result != null)
            {
                _logger.LogWarning("Conflicto al crear instrumento, ya existe un instrumento con el mismo nombre: '{Name}'", command.Name);
                results.Result = false;
                return results.AddError(ErrorCode.CONFLICT_ERROR, $"Ya existe: '{command.Name}'");
            }

            results.Result = true;
            return results;
        }
    }
}
