using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Application.UseCases
{
    public interface IInstrumentUseCase
    {
        Task<Results<IList<Instrument>>> GetAllAsync();
        Task<Results<Instrument>> CreateAsync(Instrument instrument);
    }
}
