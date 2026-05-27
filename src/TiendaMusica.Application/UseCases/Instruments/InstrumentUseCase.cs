using Microsoft.Extensions.Logging;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Validators.Instruments;
using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Events;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Application.UseCases.Instruments
{
    public class InstrumentUseCase : IInstrumentUseCase
    {
        private readonly IInstrumentsRepositoryPort _instrumentsRepositoryPorts;
        private readonly ILogger<InstrumentUseCase> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DomainEventsCollector _events;
        private readonly IInstrumentValidator<InstrumentUpdateCommand, Instrument> _updateValidator;
        private readonly IInstrumentValidator<InstrumentCreateCommand, bool> _createValidator;
        private readonly IInstrumentValidator<InstrumentDeleteMultipleCommand, IList<Instrument>> _deleteMassiveValidator;

        public InstrumentUseCase(
            IInstrumentsRepositoryPort instrumentsRepositoryPorts,
            IInstrumentValidator<InstrumentUpdateCommand, Instrument> updateValidator,
            IInstrumentValidator<InstrumentCreateCommand, bool> createValidator,
            IInstrumentValidator<InstrumentDeleteMultipleCommand, IList<Instrument>> deleteMassiveValidator,
            ILogger<InstrumentUseCase> logger,
            IUnitOfWork unitOfWork,
            DomainEventsCollector events
            )
        {
            _instrumentsRepositoryPorts = instrumentsRepositoryPorts;
            _updateValidator = updateValidator;
            _createValidator = createValidator;
            _deleteMassiveValidator = deleteMassiveValidator;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _events = events;
        }

        public async Task<Results<IList<Instrument>>> GetAllAsync(InstrumentGetAllQueryParametersDto? query)
        {
            _logger.LogInformation("Inicialización Obtención de todos los instrumentos desde el caso de uso");

            var results = new Results<IList<Instrument>>();

            var resultInstruments = await _instrumentsRepositoryPorts.GetAllAsync(query);

            if (resultInstruments.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores llamando al respositorio sql server:{Errors}", resultInstruments.Errors);
                results.AddErrors(resultInstruments.Errors);
                return results;
            }

            _logger.LogInformation("Retornando todos los instrumentos exitosamente con {Count} instrumentos desde el caso de uso", resultInstruments.Result.Count);
            results.Result = resultInstruments.Result;
            return results;
        }

        public async Task<Results<Instrument>> GetByIdAsync(string id)
        {
            var results = new Results<Instrument>();
            _logger.LogInformation("Inicialización Obtención de instrumento por ID desde el caso de uso con ID: {InstrumentId}", id);

            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("Error validación ID: ID está vacío");
                results.AddError(ErrorCode.VALIDATION_ERROR, "El ID del instrumento no puede estar vacío");
                return results;
            }

            var resultFind = await _instrumentsRepositoryPorts.GetByIdAsync(id);

            if (resultFind.HasErrors || !resultFind.IsSuccess)
            {
                _logger.LogWarning("Se encontraron errores llamando al repositorio para obtener instrumento por ID:{Errors}", resultFind.Errors);
                results.AddErrors(resultFind.Errors);
                return results;
            }

            if (resultFind.Result == null)
            {
                _logger.LogWarning("Instrumento no encontrado con ID: {InstrumentId}", id);
                results.AddError(ErrorCode.NOT_FOUND, $"Instrumento no encontrado con ID: {id}");
                return results;
            }

            _logger.LogInformation("Retornando instrumento exitosamente desde el caso de uso con ID: {InstrumentId}", id);
            results.Result = resultFind.Result;
            return results;
        }

        public async Task<Results<Instrument>> CreateAsync(InstrumentCreateCommand instrumentCommand)
        {
            _logger.LogInformation("Inicialización creación instrumento desde el caso de uso");
            var results = new Results<Instrument>();

            try
            {
                var validationResults = await _createValidator.ValidateAsync(instrumentCommand);
                if (validationResults.HasErrors || !validationResults.IsSuccess)
                {
                    _logger.LogWarning("Error validación: {Errors}", validationResults.Errors);
                    return results.AddErrors(validationResults.Errors);
                }

                var instrument = Instrument.Create(
                    instrumentCommand.Name,
                    instrumentCommand.Description,
                    instrumentCommand.Type,
                    instrumentCommand.Price,
                    instrumentCommand.Stock
                    );

                if (instrument.HasErrors)
                {
                    _logger.LogWarning("Error validación creación de instrumento en dominio: {Errors}", instrument.Errors);
                    return results.AddErrors(instrument.Errors);
                }

                var resultCreate = await _instrumentsRepositoryPorts.CreateAsync(instrument.Result);

                if (resultCreate.HasErrors)
                {
                    _logger.LogWarning("Se encontraron errores llamando al respositorio sql server para crear instrumento:{Errors}", resultCreate.Errors);
                    return results.AddErrors(resultCreate.Errors);
                }

                var saveChangesResult = await _unitOfWork.SaveChangesAsync<string>();

                if (saveChangesResult.HasErrors || !saveChangesResult.Result)
                {
                    _logger.LogWarning("Se encontraron errores al guardar los cambios en la base de datos después de crear el instrumento:{Errors}", saveChangesResult.Errors);
                    return results.AddErrors(saveChangesResult.Errors);
                }

                _logger.LogInformation("Instrumento creado exitosamente con ID: {InstrumentId}", resultCreate.Result.Id);

                return resultCreate;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error al contruir objeto de dominio: {Message}", ex.Message);
                return results.AddError(ErrorCode.VALIDATION_ERROR, $"Error Domain: {ex.Message}");
            }
        }

        public async Task<Results<Instrument>> UpdateAsync(InstrumentUpdateCommand command)
        {
            _logger.LogInformation("Inicialización actualización de instrumento: {InstrumentId}", command.Id);
            var results = new Results<Instrument>();

            try
            {
                var validationResult = await _updateValidator.ValidateAsync(command);

                if (validationResult.HasErrors)
                {
                    _logger.LogWarning("Error validación: {Errors}", validationResult.Errors);
                    return results.AddErrors(validationResult.Errors);
                }

                var instrumentToUpdate = validationResult.Result;

                var instrumentUpdated = instrumentToUpdate.Update(
                    command.Name,
                    command.Description,
                    command.Type
                );

                if (instrumentUpdated.HasErrors)
                {
                    _logger.LogWarning("Error domain: {Errors}", instrumentUpdated.Errors);
                    return results.AddErrors(instrumentUpdated.Errors);
                }

                _instrumentsRepositoryPorts.Update(instrumentUpdated.Result);

                var saveResult = await _unitOfWork.SaveChangesAsync<string>();

                if (saveResult.HasErrors || !saveResult.Result)
                {
                    _logger.LogWarning("Se encontraron errores al guardar los cambios en la base de datos después de actualizar el instrumento:{Errors}", saveResult.Errors);
                    return results.AddErrors(saveResult.Errors);
                }

                _logger.LogInformation("Actualizado: {InstrumentId}", instrumentUpdated.Result.Id);
                return instrumentUpdated;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error: {Message}", ex.Message);
                return results.AddError(ErrorCode.VALIDATION_ERROR, $"Error: {ex.Message}");
            }
        }

        public async Task<Results<int>> DeleteMultipleAsync(InstrumentDeleteMultipleCommand command)
        {
            _logger.LogInformation("Inicialización eliminación masiva de instrumentos desde el caso de uso con {Count} IDs", command.InstrumentIds.Count);
            var results = new Results<int>();

            var instrumentsToDelete = await _deleteMassiveValidator.ValidateAsync(command);

            if (instrumentsToDelete.HasErrors || !instrumentsToDelete.IsSuccess)
            {
                _logger.LogWarning("Error validación: {Errors}", instrumentsToDelete.Errors);
                return results.AddErrors(instrumentsToDelete.Errors);
            }

            _logger.LogInformation("Iniciando eliminación masiva en el repositorio");
            _instrumentsRepositoryPorts.DeleteMultiple(instrumentsToDelete.Result);

            _logger.LogInformation("Agregando evento de eliminación masiva de instrumentos");
            _events.AddEvent(new InstrumentDeletedMassiveEvent(instrumentsToDelete.Result));

            var saveChangesResult = await _unitOfWork.SaveChangesAsync<string>();

            if (saveChangesResult.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores al guardar los cambios en la base de datos después de crear el instrumento:{Errors}", saveChangesResult.Errors);
                return results.AddErrors(saveChangesResult.Errors);
            }

            _logger.LogInformation("Eliminación masiva completada exitosamente. {Count} instrumentos eliminados", instrumentsToDelete.Result.Count);
            results.Result = instrumentsToDelete.Result.Count;
            return results;
        }
    }
}
