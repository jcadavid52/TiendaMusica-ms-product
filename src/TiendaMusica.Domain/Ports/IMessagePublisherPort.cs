namespace TiendaMusica.Domain.Ports
{
    public interface IMessagePublisherPort
    {
        Task PublishAsync<T>(T @event) where T : class;
    }
}
