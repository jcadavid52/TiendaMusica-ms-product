using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Config;

namespace TiendaMusica.Tests.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Config
{
    public class RedisConfig_UTest
    {
        [Fact]
        public void ShouldHaveDefaultValues()
        {
            var config = new RedisConfig();

            Assert.Equal(string.Empty, config.ConnectionString);
            Assert.Equal(60, config.DefaultExpirationMinutes);
            Assert.True(config.Enabled);
            Assert.Equal(1000, config.ConnectTimeoutMilliseconds);
            Assert.Equal(1000, config.SyncTimeoutMilliseconds);
        }

        [Fact]
        public void ShouldStoreCustomValues()
        {
            var config = new RedisConfig
            {
                ConnectionString = "localhost:6379",
                DefaultExpirationMinutes = 30,
                Enabled = false,
                ConnectTimeoutMilliseconds = 2000,
                SyncTimeoutMilliseconds = 3000
            };

            Assert.Equal("localhost:6379", config.ConnectionString);
            Assert.Equal(30, config.DefaultExpirationMinutes);
            Assert.False(config.Enabled);
            Assert.Equal(2000, config.ConnectTimeoutMilliseconds);
            Assert.Equal(3000, config.SyncTimeoutMilliseconds);
        }
    }
}
