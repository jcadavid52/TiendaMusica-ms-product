using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Application.UseCases.Instruments.Validators
{
    public interface IGenericValidator<Command, Response>
    {
        Task<Results<Response>> ValidateAsync(Command command);
    }
}
