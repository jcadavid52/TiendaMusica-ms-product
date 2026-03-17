using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Domain.Services
{
    public interface IInstrumentCreateValidationService
    {
        Results<bool> ValidateLimitStockByType(int stock, int currentStock, InstrumentType type);
    }
}
