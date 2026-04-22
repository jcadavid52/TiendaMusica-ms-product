using Microsoft.Extensions.Logging;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Domain.Services;

namespace TiendaMusica.Application.Validators.Instruments
{
    public class InstrumentUpdateValidator : IInstrumentValidator<InstrumentUpdateCommand, Instrument>
    {
        private readonly IInstrumentsRepositoryPort _repository;
        private readonly IInstrumentValidationService _validationService;
        private readonly ILogger<InstrumentUpdateValidator> _logger;

        public InstrumentUpdateValidator(
            IInstrumentsRepositoryPort repository,
            IInstrumentValidationService validationService,
            ILogger<InstrumentUpdateValidator> logger)
        {
            _repository = repository;
            _validationService = validationService;
            _logger = logger;
        }
        public async Task<Results<Instrument>> ValidateAsync(InstrumentUpdateCommand command)
        {
            _logger.LogInformation("Iniciando validación para actualización de instrumento: {InstrumentId}", command.Id);
            var results = new Results<Instrument>();

            var existing = await _repository.GetByIdAsync(command.Id);
            if (existing.HasErrors)
            {
                _logger.LogWarning("Error al obtener instrumento existente: {Errors}", existing.Errors);
                return results.AddErrors(existing.Errors);
            }

            if (existing.Result == null)
            {
                _logger.LogWarning("Instrumento no encontrado con ID: {InstrumentId}", command.Id);
                return results.AddError(ErrorCode.NOT_FOUND, $"No encontrado: {command.Id}");
            }

            var nameConflict = await _repository.GetByNameAsync(command.Name);
            if (nameConflict.HasErrors)
            {
                _logger.LogWarning("Error al buscar conflicto de nombre: {Errors}", nameConflict.Errors);
                return results.AddErrors(nameConflict.Errors);
            }

            if (nameConflict.Result != null && nameConflict.Result.Id != command.Id)
            {
                _logger.LogWarning("Conflicto de nombre: ya existe '{Name}'", command.Name);
                return results.AddError(ErrorCode.CONFLICT_ERROR, $"Ya existe: {command.Name}");
            }

            var stockResult = await _repository.GetStockByType(command.Type);
            if (stockResult.HasErrors)
            {
                _logger.LogWarning("Error al obtener stock por tipo: {Errors}", stockResult.Errors);
                return results.AddErrors(stockResult.Errors);
            }

            var stockValidation = _validationService.ValidateLimitStockByType(
                existing.Result.Stock, stockResult.Result, command.Type);

            if (!stockValidation.IsSuccess && stockValidation.HasErrors)
            {
                _logger.LogWarning("Error validación de stock por tipo '{Type}': {Errors}", command.Type, stockValidation.Errors);
                return results.AddErrors(stockValidation.Errors);
            }

            if(command.Type != existing.Result.Type)
            {
                var currentStockResult = await _repository.GetStockByType(existing.Result.Type);

                if (currentStockResult.HasErrors)
                {
                    _logger.LogWarning("Error al obtener stock por tipo actual: {Errors}", currentStockResult.Errors);
                    return results.AddErrors(currentStockResult.Errors);
                }

               var minimumStockValidation = _validationService.ValidateMinimumStockAfterUpdate(
                    currentStockResult.Result, existing.Result.Type);

                if (!minimumStockValidation.IsSuccess && minimumStockValidation.HasErrors)
                {
                    _logger.LogWarning("Error validación de stock mínimo por tipo '{Type}': {Errors}", existing.Result.Type, minimumStockValidation.Errors);
                    return results.AddErrors(minimumStockValidation.Errors);
                }
            }

            _logger.LogInformation("Validación exitosa para instrumento: {InstrumentId}", command.Id);
            results.Result = existing.Result;
            return results;
        }
    }
}
