using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Domain.Ports
{
    public interface IMessagePublisherPort
    {
        Task<Results<bool>> PublishAsync<T>(T @event) where T : class;
    }
}
