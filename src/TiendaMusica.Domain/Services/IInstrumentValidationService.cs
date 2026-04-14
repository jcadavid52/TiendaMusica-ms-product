using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Domain.Services
{
    public interface IInstrumentValidationService
    {
        Results<bool> ValidateLimitStockByType(int stock, int currentStock, InstrumentType type);
        Results<bool> ValidateStockAfterDeletion(
            IList<InstrumentStockSummary> instrumentStockSummaries,
            IList<Instrument> currentInstruments
            );
    }
}
