using TiendaMusica.Domain.Models.Result;

namespace TiendaMusica.Domain.Ports
{
    public interface ICachePort
    {
        Task<Results<T?>> GetAsync<T>(string key) where T : class;
        Task<bool> ExistsAsync(string key);
        Task<Results<bool>> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task<Results<bool>> RemoveAsync(string key);
        Task<Results<bool>> RemoveByPatternAsync(string pattern);
    }
}
