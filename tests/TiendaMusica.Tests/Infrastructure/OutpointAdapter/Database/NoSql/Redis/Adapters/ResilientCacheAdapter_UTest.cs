using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters;
using System.Net;

namespace TiendaMusica.Tests.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters
{
    public class ResilientCacheAdapter_UTest
    {
        private readonly Mock<IConnectionMultiplexer> _redisMock;
        private readonly Mock<ICachePort> _innerCacheMock;
        private readonly Mock<ILogger<ResilientCacheAdapter>> _loggerMock;
        private readonly ResilientCacheAdapter _adapter;

        public ResilientCacheAdapter_UTest()
        {
            _redisMock = new Mock<IConnectionMultiplexer>();
            _innerCacheMock = new Mock<ICachePort>();
            _loggerMock = new Mock<ILogger<ResilientCacheAdapter>>();

            _redisMock.Setup(r => r.IsConnected).Returns(true);

            _adapter = new ResilientCacheAdapter(
                _redisMock.Object,
                _innerCacheMock.Object,
                _loggerMock.Object,
                timeoutMs: 5000
            );
        }

        [Fact]
        public async Task GetAsync_WhenRedisAvailable_ShouldReturnFromInnerCache()
        {
            var expected = new Results<string?> { Result = "cached" };
            _innerCacheMock.Setup(c => c.GetAsync<string>("key"))
                .ReturnsAsync(expected);

            var result = await _adapter.GetAsync<string>("key");

            Assert.NotNull(result);
            Assert.Equal("cached", result.Result);
            _innerCacheMock.Verify(c => c.GetAsync<string>("key"), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WhenRedisNotAvailable_ShouldFallbackToNullCache()
        {
            _redisMock.Setup(r => r.IsConnected).Returns(false);
            var adapter = new ResilientCacheAdapter(
                _redisMock.Object,
                _innerCacheMock.Object,
                _loggerMock.Object,
                timeoutMs: 5000
            );

            var result = await adapter.GetAsync<string>("key");

            Assert.NotNull(result);
            Assert.Null(result.Result);
            Assert.True(result.IsSuccess);
            _innerCacheMock.Verify(c => c.GetAsync<string>(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_WhenRedisThrowsException_ShouldFallbackAndMarkRedisUnavailable()
        {
            _innerCacheMock.Setup(c => c.GetAsync<string>("key"))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "error"));

            var result = await _adapter.GetAsync<string>("key");

            Assert.NotNull(result);
            Assert.Null(result.Result);
            Assert.True(result.IsSuccess);

            var secondResult = await _adapter.GetAsync<string>("key");
            Assert.NotNull(secondResult);
            Assert.Null(secondResult.Result);
            Assert.True(secondResult.IsSuccess);
            _innerCacheMock.Verify(c => c.GetAsync<string>("key"), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WhenTimeout_ShouldFallbackAndMarkRedisUnavailable()
        {
            var tcs = new TaskCompletionSource<Results<string?>>();
            _innerCacheMock.Setup(c => c.GetAsync<string>("key"))
                .Returns(tcs.Task);

            var timeoutAdapter = new ResilientCacheAdapter(
                _redisMock.Object,
                _innerCacheMock.Object,
                _loggerMock.Object,
                timeoutMs: 1
            );

            var result = await timeoutAdapter.GetAsync<string>("key");

            Assert.NotNull(result);
            Assert.Null(result.Result);
            Assert.True(result.IsSuccess);

            var secondResult = await timeoutAdapter.GetAsync<string>("key");
            Assert.NotNull(secondResult);
            Assert.Null(secondResult.Result);
            _innerCacheMock.Verify(c => c.GetAsync<string>("key"), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WhenRedisAvailable_ShouldReturnFromInnerCache()
        {
            _innerCacheMock.Setup(c => c.ExistsAsync("key"))
                .ReturnsAsync(true);

            var result = await _adapter.ExistsAsync("key");

            Assert.True(result);
            _innerCacheMock.Verify(c => c.ExistsAsync("key"), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WhenRedisNotAvailable_ShouldReturnFalse()
        {
            _redisMock.Setup(r => r.IsConnected).Returns(false);
            var adapter = new ResilientCacheAdapter(
                _redisMock.Object,
                _innerCacheMock.Object,
                _loggerMock.Object,
                timeoutMs: 5000
            );

            var result = await adapter.ExistsAsync("key");

            Assert.False(result);
            _innerCacheMock.Verify(c => c.ExistsAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExistsAsync_WhenRedisThrowsException_ShouldReturnFalseAndMarkRedisUnavailable()
        {
            _innerCacheMock.Setup(c => c.ExistsAsync("key"))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "error"));

            var result = await _adapter.ExistsAsync("key");
            Assert.False(result);

            var secondResult = await _adapter.ExistsAsync("key");
            Assert.False(secondResult);
            _innerCacheMock.Verify(c => c.ExistsAsync("key"), Times.Once);
        }

        [Fact]
        public async Task SetAsync_WhenRedisAvailable_ShouldReturnFromInnerCache()
        {
            _innerCacheMock.Setup(c => c.SetAsync("key", "value", It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new Results<bool> { Result = true });

            var result = await _adapter.SetAsync("key", "value");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
            _innerCacheMock.Verify(c => c.SetAsync("key", "value", null), Times.Once);
        }

        [Fact]
        public async Task SetAsync_WhenRedisNotAvailable_ShouldFallbackToNullCache()
        {
            _redisMock.Setup(r => r.IsConnected).Returns(false);
            var adapter = new ResilientCacheAdapter(
                _redisMock.Object,
                _innerCacheMock.Object,
                _loggerMock.Object,
                timeoutMs: 5000
            );

            var result = await adapter.SetAsync("key", "value");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
            _innerCacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()), Times.Never);
        }

        [Fact]
        public async Task SetAsync_WhenRedisThrowsException_ShouldFallbackAndMarkRedisUnavailable()
        {
            _innerCacheMock.Setup(c => c.SetAsync("key", "value", It.IsAny<TimeSpan?>()))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "error"));

            var result = await _adapter.SetAsync("key", "value");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.Result);

            var secondResult = await _adapter.SetAsync("key", "value");
            Assert.NotNull(secondResult);
            Assert.True(secondResult.IsSuccess);
            Assert.True(secondResult.Result);
            _innerCacheMock.Verify(c => c.SetAsync("key", "value", null), Times.Once);
        }

        [Fact]
        public async Task SetAsync_WhenTimeout_ShouldFallbackAndMarkRedisUnavailable()
        {
            var tcs = new TaskCompletionSource<Results<bool>>();
            _innerCacheMock.Setup(c => c.SetAsync("key", "value", It.IsAny<TimeSpan?>()))
                .Returns(tcs.Task);

            var timeoutAdapter = new ResilientCacheAdapter(
                _redisMock.Object,
                _innerCacheMock.Object,
                _loggerMock.Object,
                timeoutMs: 1
            );

            var result = await timeoutAdapter.SetAsync("key", "value");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.Result);

            var secondResult = await timeoutAdapter.SetAsync("key", "value");
            Assert.NotNull(secondResult);
            Assert.True(secondResult.IsSuccess);
            Assert.True(secondResult.Result);
            _innerCacheMock.Verify(c => c.SetAsync("key", "value", null), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_WhenRedisAvailable_ShouldReturnFromInnerCache()
        {
            _innerCacheMock.Setup(c => c.RemoveAsync("key"))
                .ReturnsAsync(new Results<bool> { Result = true });

            var result = await _adapter.RemoveAsync("key");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
        }

        [Fact]
        public async Task RemoveAsync_WhenRedisNotAvailable_ShouldFallbackToNullCache()
        {
            _redisMock.Setup(r => r.IsConnected).Returns(false);
            var adapter = new ResilientCacheAdapter(
                _redisMock.Object,
                _innerCacheMock.Object,
                _loggerMock.Object,
                timeoutMs: 5000
            );

            var result = await adapter.RemoveAsync("key");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
            _innerCacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoveAsync_WhenRedisThrowsException_ShouldFallbackAndMarkRedisUnavailable()
        {
            _innerCacheMock.Setup(c => c.RemoveAsync("key"))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "error"));

            var result = await _adapter.RemoveAsync("key");
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.Result);

            var secondResult = await _adapter.RemoveAsync("key");
            Assert.NotNull(secondResult);
            Assert.True(secondResult.IsSuccess);
            Assert.True(secondResult.Result);
            _innerCacheMock.Verify(c => c.RemoveAsync("key"), Times.Once);
        }

        [Fact]
        public async Task RemoveByPatternAsync_WhenRedisAvailable_ShouldReturnFromInnerCache()
        {
            _innerCacheMock.Setup(c => c.RemoveByPatternAsync("pattern*"))
                .ReturnsAsync(new Results<bool> { Result = true });

            var result = await _adapter.RemoveByPatternAsync("pattern*");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
        }

        [Fact]
        public async Task RemoveByPatternAsync_WhenRedisNotAvailable_ShouldFallbackToNullCache()
        {
            _redisMock.Setup(r => r.IsConnected).Returns(false);
            var adapter = new ResilientCacheAdapter(
                _redisMock.Object,
                _innerCacheMock.Object,
                _loggerMock.Object,
                timeoutMs: 5000
            );

            var result = await adapter.RemoveByPatternAsync("pattern*");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
            _innerCacheMock.Verify(c => c.RemoveByPatternAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoveByPatternAsync_WhenRedisThrowsException_ShouldFallbackAndMarkRedisUnavailable()
        {
            _innerCacheMock.Setup(c => c.RemoveByPatternAsync("pattern*"))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "error"));

            var result = await _adapter.RemoveByPatternAsync("pattern*");
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.Result);

            var secondResult = await _adapter.RemoveByPatternAsync("pattern*");
            Assert.NotNull(secondResult);
            Assert.True(secondResult.IsSuccess);
            Assert.True(secondResult.Result);
            _innerCacheMock.Verify(c => c.RemoveByPatternAsync("pattern*"), Times.Once);
        }

        [Fact]
        public async Task ConnectionFailedEvent_ShouldTriggerFallback()
        {
            _redisMock.Raise(r => r.ConnectionFailed += null,
                null!,
                new ConnectionFailedEventArgs(
                    null!,
                    new IPEndPoint(IPAddress.Loopback, 6379),
                    (ConnectionType)1,
                    ConnectionFailureType.UnableToConnect,
                    new Exception("test"),
                    "physical"));

            var result = await _adapter.GetAsync<string>("key");

            Assert.Null(result.Result);
            _innerCacheMock.Verify(c => c.GetAsync<string>(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ConnectionRestoredEvent_ShouldRestoreRedisAvailability()
        {
            _redisMock.Setup(r => r.IsConnected).Returns(false);
            var adapter = new ResilientCacheAdapter(
                _redisMock.Object,
                _innerCacheMock.Object,
                _loggerMock.Object,
                timeoutMs: 5000
            );

            var fallbackResult = await adapter.ExistsAsync("key");
            Assert.False(fallbackResult);

            _innerCacheMock.Setup(c => c.ExistsAsync("key")).ReturnsAsync(true);

            _redisMock.Setup(r => r.IsConnected).Returns(true);
            _redisMock.Raise(r => r.ConnectionRestored += null,
                null!,
                new ConnectionFailedEventArgs(
                    null!,
                    new IPEndPoint(IPAddress.Loopback, 6379),
                    (ConnectionType)1,
                    ConnectionFailureType.None,
                    new Exception("restored"),
                    "physical"));

            var restoredResult = await adapter.ExistsAsync("key");
            Assert.True(restoredResult);
            _innerCacheMock.Verify(c => c.ExistsAsync("key"), Times.Once);
        }
    }
}
