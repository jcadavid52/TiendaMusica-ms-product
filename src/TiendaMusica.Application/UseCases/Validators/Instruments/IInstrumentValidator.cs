using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Application.UseCases.Validators.Instruments
{
    public interface IInstrumentValidator<Command, Response>
    {
        Task<Results<Response>> ValidateAsync(Command command);
    }
}
