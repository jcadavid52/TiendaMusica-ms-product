using Microsoft.Extensions.Logging;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Domain.Services;

namespace TiendaMusica.Application.UseCases.Instruments.Validators
{
    public class DeleteMassiveValidator : IGenericValidator<InstrumentDeleteMultipleCommand, IList<Instrument>>
    {
        private readonly IInstrumentsRepositoryPort _repository;
        private readonly IInstrumentValidationService _validationService;
        private readonly ILogger<DeleteMassiveValidator> _logger;

        public DeleteMassiveValidator(
            IInstrumentsRepositoryPort repository,
            IInstrumentValidationService validationService,
            ILogger<DeleteMassiveValidator> logger
            )
        {
            _repository = repository;
            _validationService = validationService;
            _logger = logger;
        }

        public async Task<Results<IList<Instrument>>> ValidateAsync(InstrumentDeleteMultipleCommand command)
        {
            var results = new Results<IList<Instrument>>();

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

            var toDelete = await _repository.GetByIdsAsync(command.InstrumentIds);

            if (toDelete.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores llamando al respositorio sql server para obtener instrumentos por IDs:{Errors}", toDelete.Errors);
                return results.AddErrors(toDelete.Errors);
            }

            if (toDelete.Result.Count != command.InstrumentIds.Distinct().Count())
            {
                _logger.LogWarning("No se encontraron todos los instrumentos para eliminar. IDs solicitados: {RequestedIds}, IDs encontrados: {FoundIds}", string.Join(", ", command.InstrumentIds), string.Join(", ", toDelete.Result.Select(p => p.Id)));
                var idsFounds = toDelete.Result.Select(p => p.Id);
                var idsMissing = command.InstrumentIds.Except(idsFounds);
                return results.AddError(ErrorCode.NOT_FOUND, $"No se encontraron los registros con IDs: {string.Join(", ", idsMissing)}");
            }

            var resultStockSummaries = await _repository.GetStockSummaryByInstrumentTypesAsync(command.InstrumentIds);

            if (resultStockSummaries.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores llamando al respositorio sql server para obtener el resumen de stock por tipos de instrumentos:{Errors}", resultStockSummaries.Errors);
                return results.AddErrors(resultStockSummaries.Errors);
            }

            var resultValidate = _validationService.ValidateMinimumStockAfterDeletion(resultStockSummaries.Result, toDelete.Result);

            if (resultValidate.HasErrors)
            {
                _logger.LogWarning("Error validación de stock antes de eliminación masiva: {Errors}", resultValidate.Errors);
                return results.AddErrors(resultValidate.Errors);
            }

            results.Result = toDelete.Result;
            return results;
        }
    }
}
