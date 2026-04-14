using TiendaMusica.Application.Dtos;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Application.UseCases.Instruments
{
    public interface IInstrumentUseCase
    {
        Task<Results<IList<Instrument>>> GetAllAsync(InstrumentGetAllQuery? query = null);
        Task<Results<Instrument>> GetByIdAsync(string id);
        Task<Results<Instrument>> CreateAsync(InstrumentCreateCommand instrument);
        Task<Results<Instrument>> UpdateAsync(InstrumentUpdateCommand command);
        Task<Results<int>> DeleteMultipleAsync(InstrumentDeleteMultipleCommand command);
    }
}
