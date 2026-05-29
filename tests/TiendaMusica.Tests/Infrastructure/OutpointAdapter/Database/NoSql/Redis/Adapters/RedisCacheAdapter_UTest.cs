using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using System.Net;
using System.Text.Json;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Config;

namespace TiendaMusica.Tests.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters
{
    public class RedisCacheAdapter_UTest
    {
        private readonly Mock<IConnectionMultiplexer> _redisMock;
        private readonly Mock<IDatabase> _databaseMock;
        private readonly Mock<IOptions<RedisConfig>> _configMock;
        private readonly RedisCacheAdapter _adapter;

        public RedisCacheAdapter_UTest()
        {
            _redisMock = new Mock<IConnectionMultiplexer>();
            _databaseMock = new Mock<IDatabase>();
            _configMock = new Mock<IOptions<RedisConfig>>();

            _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_databaseMock.Object);

            _configMock.Setup(c => c.Value).Returns(new RedisConfig
            {
                ConnectionString = "localhost:6379",
                DefaultExpirationMinutes = 60,
                Enabled = true
            });

            _adapter = new RedisCacheAdapter(_redisMock.Object, _configMock.Object);
        }

        [Fact]
        public async Task GetAsync_WhenKeyExists_ShouldDeserializeValue()
        {
            var testObj = new TestData { Name = "test", Value = 42 };
            var json = JsonSerializer.Serialize(testObj);
            _databaseMock.Setup(d => d.StringGetAsync("mykey", It.IsAny<CommandFlags>()))
                .ReturnsAsync(new RedisValue(json));

            var result = await _adapter.GetAsync<TestData>("mykey");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
            Assert.Equal("test", result.Result.Name);
            Assert.Equal(42, result.Result.Value);
        }

        [Fact]
        public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnNull()
        {
            _databaseMock.Setup(d => d.StringGetAsync("missing", It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            var result = await _adapter.GetAsync<TestData>("missing");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task GetAsync_WhenRedisThrowsException_ShouldReturnDatabaseError()
        {
            _databaseMock.Setup(d => d.StringGetAsync("error", It.IsAny<CommandFlags>()))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "connection error"));

            var result = await _adapter.GetAsync<TestData>("error");

            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.DATABASE_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
        {
            _databaseMock.Setup(d => d.KeyExistsAsync("existing", It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            var result = await _adapter.ExistsAsync("existing");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenKeyDoesNotExist_ShouldReturnFalse()
        {
            _databaseMock.Setup(d => d.KeyExistsAsync("missing", It.IsAny<CommandFlags>()))
                .ReturnsAsync(false);

            var result = await _adapter.ExistsAsync("missing");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenRedisThrowsException_ShouldReturnFalse()
        {
            _databaseMock.Setup(d => d.KeyExistsAsync("error", It.IsAny<CommandFlags>()))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "error"));

            var result = await _adapter.ExistsAsync("error");

            Assert.False(result);
        }

        [Fact]
        public async Task SetAsync_ShouldSerializeAndStoreValue()
        {
            var testObj = new TestData { Name = "test", Value = 42 };
            _databaseMock.Setup(d => d.StringSetAsync("mykey", It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            var result = await _adapter.SetAsync("mykey", testObj);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
            _databaseMock.Verify(d => d.StringSetAsync("mykey", It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), false, When.Always, CommandFlags.None), Times.Once);
        }

        [Fact]
        public async Task SetAsync_WhenRedisThrowsException_ShouldReturnDatabaseError()
        {
            _databaseMock.Setup(d => d.StringSetAsync("error", It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "error"));

            var result = await _adapter.SetAsync("error", "value");

            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.False(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.DATABASE_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task SetAsync_ShouldUseCustomExpiration()
        {
            var expiration = TimeSpan.FromMinutes(5);
            _databaseMock.Setup(d => d.StringSetAsync("key", It.IsAny<RedisValue>(), expiration, It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            var result = await _adapter.SetAsync("key", "value", expiration);

            Assert.True(result.IsSuccess);
            _databaseMock.Verify(d => d.StringSetAsync("key", It.IsAny<RedisValue>(), expiration, false, When.Always, CommandFlags.None), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_ShouldDeleteKey()
        {
            _databaseMock.Setup(d => d.KeyDeleteAsync("mykey", It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            var result = await _adapter.RemoveAsync("mykey");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
        }

        [Fact]
        public async Task RemoveAsync_WhenRedisThrowsException_ShouldReturnDatabaseError()
        {
            _databaseMock.Setup(d => d.KeyDeleteAsync("error", It.IsAny<CommandFlags>()))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "error"));

            var result = await _adapter.RemoveAsync("error");

            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.False(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.DATABASE_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task RemoveByPatternAsync_ShouldDeleteMatchingKeys()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, 6379);
            var serverMock = new Mock<IServer>();

            _databaseMock.Setup(d => d.Multiplexer).Returns(_redisMock.Object);
            _redisMock.Setup(r => r.GetEndPoints(It.IsAny<bool>())).Returns(new EndPoint[] { endPoint });
            _redisMock.Setup(r => r.GetServer(endPoint, It.IsAny<object>())).Returns(serverMock.Object);

            var redisKeys = new RedisKey[] { "key1", "key2" };
            serverMock.Setup(s => s.Keys(It.IsAny<int>(), It.IsAny<RedisValue>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>()))
                .Returns(redisKeys);

            _databaseMock.Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(2);

            var result = await _adapter.RemoveByPatternAsync("pattern*");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
        }

        [Fact]
        public async Task RemoveByPatternAsync_WhenNoKeysMatch_ShouldStillReturnSuccess()
        {
            var endPoint = new IPEndPoint(IPAddress.Loopback, 6379);
            var serverMock = new Mock<IServer>();

            _databaseMock.Setup(d => d.Multiplexer).Returns(_redisMock.Object);
            _redisMock.Setup(r => r.GetEndPoints(It.IsAny<bool>())).Returns(new EndPoint[] { endPoint });
            _redisMock.Setup(r => r.GetServer(endPoint, It.IsAny<object>())).Returns(serverMock.Object);

            serverMock.Setup(s => s.Keys(It.IsAny<int>(), It.IsAny<RedisValue>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>()))
                .Returns(Array.Empty<RedisKey>());

            var result = await _adapter.RemoveByPatternAsync("nomatch*");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
            _databaseMock.Verify(d => d.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()), Times.Never);
        }

        [Fact]
        public async Task RemoveByPatternAsync_WhenRedisThrowsException_ShouldReturnDatabaseError()
        {
            _databaseMock.Setup(d => d.Multiplexer).Throws(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "error"));

            var result = await _adapter.RemoveByPatternAsync("pattern*");

            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.False(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.DATABASE_ERROR, result.Errors.First().ErrorCode);
        }

        private class TestData
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
        }
    }
}
