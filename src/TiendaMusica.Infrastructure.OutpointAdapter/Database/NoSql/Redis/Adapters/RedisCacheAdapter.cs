using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Config;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters
{
    public class RedisCacheAdapter : ICachePort
    {
        private readonly IDatabase _database;
        private readonly RedisConfig _config;

        public RedisCacheAdapter(
            IConnectionMultiplexer redis,
            IOptions<RedisConfig> config
            )
        {
            _database = redis.GetDatabase();
            _config = config.Value;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var value = await _database.StringGetAsync(key);

            if (value.IsNullOrEmpty) return null;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var jsonData = JsonSerializer.Serialize(value);
            var effectiveExpiration = expiration ?? TimeSpan.FromMinutes(_config.DefaultExpirationMinutes);
            await _database.StringSetAsync(key, jsonData, effectiveExpiration);
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).ToArray();
            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
            }
        }
    }
}
