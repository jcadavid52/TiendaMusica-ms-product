using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Application.Ports
{
    public interface IInstrumentsRepositoryPort
    {
        Results<IList<Instrument>> GetAll();
    }
}
