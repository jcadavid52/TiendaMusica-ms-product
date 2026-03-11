using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Application.Ports
{
    public interface IInstrumentsRepositoryPort
    {
        Task<Results<IList<Instrument>>> GetAllAsync();
        Task<Results<Instrument>> CreateAsync(Instrument instrument);
    }
}
