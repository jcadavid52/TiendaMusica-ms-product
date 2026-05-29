using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters;

namespace TiendaMusica.Tests.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters
{
    public class NullCacheAdapter_UTest
    {
        private readonly NullCacheAdapter _adapter;

        public NullCacheAdapter_UTest()
        {
            _adapter = new NullCacheAdapter();
        }

        [Fact]
        public async Task GetAsync_ShouldReturnEmptyResult()
        {
            var result = await _adapter.GetAsync<string>("key");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse()
        {
            var result = await _adapter.ExistsAsync("key");

            Assert.False(result);
        }

        [Fact]
        public async Task SetAsync_ShouldReturnSuccess()
        {
            var result = await _adapter.SetAsync("key", "value");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
        }

        [Fact]
        public async Task RemoveAsync_ShouldReturnSuccess()
        {
            var result = await _adapter.RemoveAsync("key");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
        }

        [Fact]
        public async Task RemoveByPatternAsync_ShouldReturnSuccess()
        {
            var result = await _adapter.RemoveByPatternAsync("pattern*");

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
        }
    }
}
