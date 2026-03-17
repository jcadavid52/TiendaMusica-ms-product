using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Domain.Services
{
    public class InstrumentCreateValidationService : IInstrumentCreateValidationService
    {
        public Results<bool> ValidateLimitStockByType(int stock, int currentStock, InstrumentType type)
        {
            var results = new Results<bool>();

            int totalStock = stock + currentStock;

            int limit = GetLimitStockByType(type);

            if (limit > 0 && totalStock >= limit)
                return results.AddError(ErrorCode.VALIDATION_ERROR, $"El límite de stock es de {limit} para instrumentos de tipo {type}");

            results.Result = true;
            return results;
        }

        private int GetLimitStockByType(InstrumentType type)
        {
            return type switch
            {
                InstrumentType.Wind => (int)InstrumentLimitStockByType.Wind,
                InstrumentType.Stringed => (int)InstrumentLimitStockByType.Stringed,
                InstrumentType.keyboard => (int)InstrumentLimitStockByType.keyboard,
                _ => 0
            };
        }
    }
}
