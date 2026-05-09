using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters
{
    public class ResilientCacheAdapter : ICachePort, IDisposable
    {
        private readonly ICachePort _innerCache;
        private readonly ICachePort _nullCache;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<ResilientCacheAdapter> _logger;
        private readonly int _timeoutMs;
        private volatile bool _isRedisAvailable;
        private bool _fallbackLogged;

        public ResilientCacheAdapter(
            IConnectionMultiplexer redis,
            ICachePort innerCache,
            ILogger<ResilientCacheAdapter> logger,
            int timeoutMs = 2000)
        {
            _redis = redis;
            _innerCache = innerCache;
            _nullCache = new NullCacheAdapter();
            _logger = logger;
            _timeoutMs = timeoutMs;
            _isRedisAvailable = IsConnected();

            _redis.ConnectionFailed += OnConnectionFailed;
            _redis.ConnectionRestored += OnConnectionRestored;
        }

        public void Dispose()
        {
            _redis.ConnectionFailed -= OnConnectionFailed;
            _redis.ConnectionRestored -= OnConnectionRestored;
        }

        private void OnConnectionFailed(object? sender, ConnectionFailedEventArgs args)
        {
            _logger.LogWarning("Redis connection failed: {ConnectionType}", args.ConnectionType);
            _isRedisAvailable = false;
        }

        private void OnConnectionRestored(object? sender, ConnectionFailedEventArgs args)
        {
            _logger.LogInformation("Redis connection restored: {ConnectionType}", args.ConnectionType);
            _isRedisAvailable = true;
            _fallbackLogged = false;
        }

        private void LogFallbackIfNeeded()
        {
            if (!_fallbackLogged)
            {
                _logger.LogWarning("Redis no está disponible, usando NullCache como fallback");
                _fallbackLogged = true;
            }
        }

        private bool IsConnected()
        {
            try
            {
                return _redis.IsConnected;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Results<T?>> GetAsync<T>(string key) where T : class
        {
            if (!_isRedisAvailable)
            {
                LogFallbackIfNeeded();
                return await _nullCache.GetAsync<T>(key);
            }

            try
            {
                var task = _innerCache.GetAsync<T>(key);
                await task.WaitAsync(TimeSpan.FromMilliseconds(_timeoutMs));
                return task.Result;
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timeout al obtener clave {Key} de Redis", key);
                _isRedisAvailable = false;
                return new Results<T?>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener clave {Key} de Redis", key);
                _isRedisAvailable = false;
                return new Results<T?>();
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (!_isRedisAvailable)
            {
                LogFallbackIfNeeded();
                return false;
            }

            try
            {
                var task = _innerCache.ExistsAsync(key);
                await task.WaitAsync(TimeSpan.FromMilliseconds(_timeoutMs));
                return task.Result;
            }
            catch
            {
                _isRedisAvailable = false;
                return false;
            }
        }

        public async Task<Results<bool>> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (!_isRedisAvailable)
            {
                LogFallbackIfNeeded();
                return await _nullCache.SetAsync(key, value, expiration);
            }

            try
            {
                var task = _innerCache.SetAsync(key, value, expiration);
                await task.WaitAsync(TimeSpan.FromMilliseconds(_timeoutMs));
                return task.Result;
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timeout al guardar clave {Key} en Redis", key);
                _isRedisAvailable = false;
                return new Results<bool> { Result = false };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al guardar clave {Key} en Redis", key);
                _isRedisAvailable = false;
                return new Results<bool> { Result = false };
            }
        }

        public async Task<Results<bool>> RemoveAsync(string key)
        {
            if (!_isRedisAvailable)
            {
                LogFallbackIfNeeded();
                return await _nullCache.RemoveAsync(key);
            }

            try
            {
                var task = _innerCache.RemoveAsync(key);
                await task.WaitAsync(TimeSpan.FromMilliseconds(_timeoutMs));
                return task.Result;
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timeout al eliminar clave {Key} en Redis", key);
                _isRedisAvailable = false;
                return new Results<bool> { Result = false };
            }
            catch
            {
                _isRedisAvailable = false;
                return new Results<bool> { Result = false };
            }
        }

        public async Task<Results<bool>> RemoveByPatternAsync(string pattern)
        {
            if (!_isRedisAvailable)
            {
                LogFallbackIfNeeded();
                return await _nullCache.RemoveByPatternAsync(pattern);
            }

            try
            {
                var task = _innerCache.RemoveByPatternAsync(pattern);
                await task.WaitAsync(TimeSpan.FromMilliseconds(_timeoutMs));
                return task.Result;
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timeout al eliminar patrón {Pattern} en Redis", pattern);
                _isRedisAvailable = false;
                return new Results<bool> { Result = false };
            }
            catch
            {
                _isRedisAvailable = false;
                return new Results<bool> { Result = false };
            }
        }
    }
}
