using System.Linq.Expressions;
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
        Task<Results<Instrument?>> GetByNameAsync(string name);
        Task<Results<int>> GetStockByType(InstrumentType type);
        Task<Results<Instrument>> CreateAsync(Instrument instrument);
    }
}
