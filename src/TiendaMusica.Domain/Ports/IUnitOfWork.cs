using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Domain.Ports
{
    public interface IUnitOfWork
    {
        Task<Results<bool>> SaveChangesAsync<TId>(CancellationToken cancellationToken = default);
    }
}
