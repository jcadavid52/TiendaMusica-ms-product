using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Application.UseCases
{
    public interface IInstrumentUseCase
    {
        Results<IList<Instrument>> GetAll();
    }
}
