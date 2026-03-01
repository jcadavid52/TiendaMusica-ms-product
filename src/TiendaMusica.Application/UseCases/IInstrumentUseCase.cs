using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Application.UseCases
{
    internal interface IInstrumentUseCase
    {
        Results<IList<Instrument>> GetAll();
    }
}
