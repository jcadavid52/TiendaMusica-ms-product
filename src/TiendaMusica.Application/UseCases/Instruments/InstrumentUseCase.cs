using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Domain.Enums;
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
        private readonly ILogger<InstrumentUseCase> _logger;
        private readonly IMessagePublisherPort _messagePublisher;
        private readonly IUnitOfWork _unitOfWork;
        public InstrumentUseCase(
            IInstrumentsRepositoryPort instrumentsRepositoryPorts,
            IInstrumentCreateValidationService instrumentCreateValidationService,
            ILogger<InstrumentUseCase> logger,
            IMessagePublisherPort messagePublisher,
            IUnitOfWork unitOfWork
            )
        {
            _instrumentsRepositoryPorts = instrumentsRepositoryPorts;
            _instrumentCreateValidationService = instrumentCreateValidationService;
            _logger = logger;
            _messagePublisher = messagePublisher;
            _unitOfWork = unitOfWork;
        }
        public async Task<Results<IList<Instrument>>> GetAllAsync(InstrumentGetAllQuery? query = null)
        {
            _logger.LogInformation("Inicialización Obtención de todos los instrumentos desde el caso de uso");

            var resultInstruments = new Results<IList<Instrument>>();

            if (query != null)
            {
                var filters = new List<Expression<Func<Instrument, bool>>>();

                if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    var searchTerm = query.Search.Trim();
                    bool isEnumValue = Enum.TryParse<InstrumentType>(searchTerm, true, out var typeResult);

                    if (isEnumValue)
                    {
                        filters.Add(b =>
                        b.Name.Contains(searchTerm) ||
                        b.Description.Contains(searchTerm) ||
                        b.Type == typeResult
                        );
                    }
                    else
                    {
                        filters.Add(b =>
                         b.Name.Contains(searchTerm) ||
                         b.Description.Contains(searchTerm)
                         );
                    }

                }

                resultInstruments = await _instrumentsRepositoryPorts.GetAllAsync(
                    skip: (query.PageNumber - 1) * query.PageSize,
                    take: query.PageSize,
                    filters: [.. filters],
                    sortDirection: query.SortDirection
                );
            }
            else
            {
                resultInstruments = await _instrumentsRepositoryPorts.GetAllAsync();
            }

            if (resultInstruments.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores llamando al respositorio sql server:{Errors}", resultInstruments.Errors);
                return new Results<IList<Instrument>>().AddErrors(resultInstruments.Errors);
            }

            _logger.LogInformation("Retornando todos los instrumentos exitosamente con {Count} instrumentos desde el caso de uso", resultInstruments.Result.Count);
            return new Results<IList<Instrument>> { Result = resultInstruments.Result };
        }

        public async Task<Results<Instrument?>> GetByIdAsync(string id)
        {
            _logger.LogInformation("Inicialización Obtención de instrumento por ID desde el caso de uso con ID: {InstrumentId}", id);

            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("Error validación ID: ID está vacío");
                return new Results<Instrument?>().AddError(ErrorCode.VALIDATION_ERROR, "El ID del instrumento no puede estar vacío");
            }

            var result = await _instrumentsRepositoryPorts.GetByIdAsync(id);

            if (result.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores llamando al repositorio para obtener instrumento por ID:{Errors}", result.Errors);
                return new Results<Instrument?>().AddErrors(result.Errors);
            }

            _logger.LogInformation("Retornando instrumento exitosamente desde el caso de uso con ID: {InstrumentId}", id);
            return result;
        }

        public async Task<Results<Instrument>> CreateAsync(InstrumentCreateCommand instrumentCommand)
        {
            _logger.LogInformation("Inicialización creación instrumento desde el caso de uso");
            var results = new Results<Instrument>();

            try
            {
                var currentLimitStockResult = await _instrumentsRepositoryPorts.GetStockByType(instrumentCommand.Type);
                if (currentLimitStockResult.HasErrors)
                {
                    _logger.LogWarning("Se encontraron errores llamando al respositorio sql server para obtener el stock actual por tipo:{Errors}", currentLimitStockResult.Errors);
                    return results.AddErrors(currentLimitStockResult.Errors);
                }

                var validation = _instrumentCreateValidationService.ValidateLimitStockByType(instrumentCommand.Stock, currentLimitStockResult.Result, instrumentCommand.Type);
                if (!validation.IsSuccess && validation.HasErrors)
                {
                    _logger.LogWarning("Error validación de stock por tipo '{Type}':{Errors}", instrumentCommand.Type, validation.Errors);
                    return results.AddErrors(validation.Errors);
                }

                var existing = await _instrumentsRepositoryPorts.GetByNameAsync(instrumentCommand.Name);
                if (existing.HasErrors)
                {
                    _logger.LogWarning("Se encontraron errores llamando al respositorio sql server para obtener instrumento por nombre:{Errors}", existing.Errors);
                    return results.AddErrors(existing.Errors);
                }

                if (existing.Result != null)
                {
                    _logger.LogWarning("Conflicto al crear instrumento, ya existe un instrumento con el mismo nombre: '{Name}'", instrumentCommand.Name);
                    return results.AddError(ErrorCode.CONFLICT_ERROR, $"Ya existe: '{instrumentCommand.Name}'");
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

                if (saveChangesResult.HasErrors)
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

        public async Task<Results<int>> DeleteMultipleAsync(InstrumentDeleteMultipleCommand command)
        {
            _logger.LogInformation("Inicialización eliminación masiva de instrumentos desde el caso de uso con {Count} IDs", command.InstrumentIds.Count);
            var results = new Results<int>();

            if (command.InstrumentIds == null || command.InstrumentIds.Count == 0)
            {
                _logger.LogWarning("Error validación: lista de IDs vacía o null");
                return results.AddError(ErrorCode.VALIDATION_ERROR, "La lista de IDs no puede estar vacía");
            }

            var invalidIds = command.InstrumentIds.Where(id => string.IsNullOrWhiteSpace(id)).ToList();
            if (invalidIds.Any())
            {
                _logger.LogWarning("Error validación: se encontraron IDs vacíos o inválidos");
                return results.AddError(ErrorCode.VALIDATION_ERROR, "Se encontraron IDs vacíos o inválidos");
            }

            var result = await _instrumentsRepositoryPorts.DeleteMultipleAsync(command.InstrumentIds);

            if (result.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores llamando al repositorio para eliminar múltiples instrumentos: {Errors}", result.Errors);
                return results.AddErrors(result.Errors);
            }

            var saveChangesResult = await _unitOfWork.SaveChangesAsync<string>();

            if (saveChangesResult.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores al guardar los cambios en la base de datos después de crear el instrumento:{Errors}", saveChangesResult.Errors);
                return results.AddErrors(saveChangesResult.Errors);
            }

            _logger.LogInformation("Eliminación masiva completada exitosamente. {Count} instrumentos eliminados", result.Result);
            results.Result = result.Result;
            return results;
        }

        public async Task<Results<Instrument>> UpdateAsync(InstrumentUpdateCommand command)
        {
            _logger.LogInformation("Inicialización actualización de instrumento desde el caso de uso con ID: {InstrumentId}", command.Id);
            var results = new Results<Instrument>();

            if (string.IsNullOrWhiteSpace(command.Id))
            {
                _logger.LogWarning("Error validación ID: ID está vacío");
                return results.AddError(ErrorCode.VALIDATION_ERROR, "El ID del instrumento no puede estar vacío");
            }

            try
            {
                var existingInstrumentResult = await _instrumentsRepositoryPorts.GetByIdAsync(command.Id);
                if (existingInstrumentResult.HasErrors)
                {
                    _logger.LogWarning("Error al obtener instrumento existente: {Errors}", existingInstrumentResult.Errors);
                    return results.AddErrors(existingInstrumentResult.Errors);
                }

                if (existingInstrumentResult.Result == null)
                {
                    _logger.LogWarning("Instrumento no encontrado con ID: {InstrumentId}", command.Id);
                    return results.AddError(ErrorCode.NOT_FOUND, $"Instrumento no encontrado con ID: {command.Id}");
                }

                var existing = await _instrumentsRepositoryPorts.GetByNameAsync(command.Name);
                if (existing.HasErrors)
                {
                    _logger.LogWarning("Se encontraron errores llamando al respositorio sql server para obtener instrumento por nombre:{Errors}", existing.Errors);
                    return results.AddErrors(existing.Errors);
                }

                if (existing.Result != null && existing.Result.Name != command.Name)
                {
                    _logger.LogWarning("Conflicto al crear instrumento, ya existe un instrumento con el mismo nombre: '{Name}'", command.Name);
                    return results.AddError(ErrorCode.CONFLICT_ERROR, $"Ya existe: '{command.Name}'");
                }

                var currentLimitStockResult = await _instrumentsRepositoryPorts.GetStockByType(command.Type);
                if (currentLimitStockResult.HasErrors)
                {
                    _logger.LogWarning("Se encontraron errores llamando al respositorio sql server para obtener el stock actual por tipo:{Errors}", currentLimitStockResult.Errors);
                    return results.AddErrors(currentLimitStockResult.Errors);
                }

                var validation = _instrumentCreateValidationService.ValidateLimitStockByType(existingInstrumentResult.Result.Stock, currentLimitStockResult.Result, command.Type);
                if (!validation.IsSuccess && validation.HasErrors)
                {
                    _logger.LogWarning("Error validación de stock por tipo '{Type}':{Errors}", command.Type, validation.Errors);
                    return results.AddErrors(validation.Errors);
                }

                var instrumentUpdated = existingInstrumentResult.Result.Update(
                    command.Name,
                    command.Description,
                    command.Type
                );

                if (instrumentUpdated.HasErrors)
                {
                    _logger.LogWarning("Error validación actualización de instrumento en dominio: {Errors}", instrumentUpdated.Errors);
                    return results.AddErrors(instrumentUpdated.Errors);
                }

                _instrumentsRepositoryPorts.Update(instrumentUpdated.Result);

                var saveChangesResult = await _unitOfWork.SaveChangesAsync<string>();

                if (saveChangesResult.HasErrors || !saveChangesResult.Result)
                {
                    _logger.LogWarning("Se encontraron errores al guardar los cambios en la base de datos después de crear el instrumento:{Errors}", saveChangesResult.Errors);
                    return results.AddErrors(saveChangesResult.Errors);
                }

                _logger.LogInformation("Instrumento actualizado exitosamente con ID: {InstrumentId}", instrumentUpdated.Result.Id);
                return instrumentUpdated;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error al actualizar instrumento: {Message}", ex.Message);
                return results.AddError(ErrorCode.VALIDATION_ERROR, $"Error Domain: {ex.Message}");
            }
        }
    }
}
