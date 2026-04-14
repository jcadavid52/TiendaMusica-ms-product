using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Domain.Services
{
    public class InstrumentValidationService : IInstrumentValidationService
    {
        public Results<bool> ValidateLimitStockByType(int stock, int currentStock, InstrumentType type)
        {
            var results = new Results<bool>();

            int totalStock = stock + currentStock;

            int limit = GetLimitStockByType(type);

            if (limit > 0 && totalStock >= limit)
                return results.AddError(ErrorCode.LIMIT_STOCK_ERROR, $"El límite de stock es de {limit} para instrumentos de tipo {type}");

            results.Result = true;

            return results;
        }

        public Results<bool> ValidateStockAfterDeletion(
            IList<InstrumentStockSummary> instrumentStockSummaries,
            IList<Instrument> currentInstruments
            )
        {
            var results = new Results<bool>();

            foreach (var stockSummary in instrumentStockSummaries)
            {
                var currentTotalStock = currentInstruments.Where(i => i.Type == stockSummary.Type).Sum(i => i.Stock);
                int minimumStock = GetMinimumStockByType(stockSummary.Type);
                if (stockSummary.TotalStock <= minimumStock || currentTotalStock >= minimumStock)
                {
                    results.Result = false;
                    return results.AddError(ErrorCode.MINIMUM_STOCK_ERROR, $"No se puede proseguir con la eliminación. El stock total de instrumentos de tipo {stockSummary.Type} es de {stockSummary.TotalStock}, y el mínimo permitido es de {minimumStock}.");
                }
            }

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
                InstrumentType.Percussion => (int)InstrumentLimitStockByType.Percussion,
                _ => 0
            };
        }

        private int GetMinimumStockByType(InstrumentType type)
        {
            return type switch
            {
                InstrumentType.Wind => (int)InstrumentTypeMinimumStockValue.Wind,
                InstrumentType.Stringed => (int)InstrumentTypeMinimumStockValue.Stringed,
                InstrumentType.keyboard => (int)InstrumentTypeMinimumStockValue.keyboard,
                InstrumentType.Percussion => (int)InstrumentTypeMinimumStockValue.Percussion,
                _ => 0
            };
        }
    }
}
