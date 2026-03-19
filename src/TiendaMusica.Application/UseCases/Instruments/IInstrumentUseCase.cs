using TiendaMusica.Application.Dtos;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Application.UseCases.Instruments
{
    public interface IInstrumentUseCase
    {
        Task<Results<IList<Instrument>>> GetAllAsync();
        Task<Results<Instrument>> CreateAsync(CreateInstrumentCommand instrument);
    }
}
