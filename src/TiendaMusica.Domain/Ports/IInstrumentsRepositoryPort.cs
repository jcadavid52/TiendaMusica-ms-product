using System.Linq.Expressions;
using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Domain.Ports
{
    public interface IInstrumentsRepositoryPort
    {
        Task<Results<IList<Instrument>>> GetAllAsync(
            SortDirection sortDirection = SortDirection.Desc,
            Expression<Func<Instrument, bool>>[]? filters = null,
            int? skip = null,
            int? take = null
            );
        Task<Results<Instrument?>> GetByIdAsync(string id);
        Task<Results<IList<Instrument>>> GetByIdsAsync(IList<string> instrumentIds);
        Task<Results<Instrument?>> GetByNameAsync(string name);
        Task<Results<int>> GetStockByType(InstrumentType type);
        Task<Results<IList<InstrumentStockSummary>>> GetStockSummaryByInstrumentTypesAsync(IList<string> instrumentIds);
        Task<Results<Instrument>> CreateAsync(Instrument instrument);
        void Update(Instrument instrument);
        void DeleteMultipleAsync(IList<Instrument> instruments);
    }
}
