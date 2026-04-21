using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using TiendaMusica.Domain.Models.Result;
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

        public async Task<Results<T?>> GetAsync<T>(string key) where T : class
        {
            var results = new Results<T?>();

            try
            {
                var value = await _database.StringGetAsync(key);

                if (value.IsNullOrEmpty)
                {
                    return results;
                }

                results.Result = JsonSerializer.Deserialize<T>(value!);
                return results;
            }
            catch (Exception ex)
            {
                results.AddError(ErrorCode.DATABASE_ERROR, $"Error al conectarse a Redis: {ex.Message}");
                return results;
            }
        }

        public async Task<Results<bool>> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var results = new Results<bool>();
            var jsonData = JsonSerializer.Serialize(value);
            var effectiveExpiration = expiration ?? TimeSpan.FromMinutes(_config.DefaultExpirationMinutes);
            try
            {
                await _database.StringSetAsync(key, jsonData, effectiveExpiration);
                results.Result = true;
            }
            catch (Exception ex)
            {
                results.Result = false;
                results.AddError(ErrorCode.DATABASE_ERROR, $"Error al conectarse a Redis: {ex.Message}");
            }
            return results;
        }
        public async Task<Results<bool>> RemoveAsync(string key)
        {
            var results = new Results<bool>();
            try
            {
                await _database.KeyDeleteAsync(key);
                results.Result = true;
            }
            catch (Exception ex)
            {
                results.Result = false;
                results.AddError(ErrorCode.DATABASE_ERROR, $"Error al conectarse a Redis: {ex.Message}");
            }
            return results;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<Results<bool>> RemoveByPatternAsync(string pattern)
        {
            var results = new Results<bool>();

            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Length > 0)
                {
                    await _database.KeyDeleteAsync(keys);
                }
                results.Result = true;
            }
            catch (Exception ex)
            {
                results.Result = false;
                results.AddError(ErrorCode.DATABASE_ERROR, $"Error al conectarse a Redis: {ex.Message}");
            }

            return results;
        }
    }
}
