using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters
{
    public class NullCacheAdapter : ICachePort
    {
        public async Task<Results<T?>> GetAsync<T>(string key) where T : class
        {
            return await Task.FromResult(new Results<T?>());
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await Task.FromResult(false);
        }

        public async Task<Results<bool>> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            return await Task.FromResult(new Results<bool> { Result = true });
        }

        public async Task<Results<bool>> RemoveAsync(string key)
        {
            return await Task.FromResult(new Results<bool> { Result = true });
        }

        public async Task<Results<bool>> RemoveByPatternAsync(string pattern)
        {
            return await Task.FromResult(new Results<bool> { Result = true });
        }
    }
}
