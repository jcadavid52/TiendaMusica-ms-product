namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Config
{
    public class RedisConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
        public int DefaultExpirationMinutes { get; set; } = 60;
        public bool Enabled { get; set; } = true;
    }
}
