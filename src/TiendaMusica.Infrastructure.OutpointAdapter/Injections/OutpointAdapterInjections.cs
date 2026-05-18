using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Reflection;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Config;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Repositories;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Config;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Repositories;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer.Repositories;
using TiendaMusica.Infrastructure.OutpointAdapter.Messaging.RabbitMq;
using TiendaMusica.Infrastructure.OutpointAdapter.Messaging.RabbitMq.Config;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Injections
{
    public static class OutpointAdapterInjections
    {
        public static IServiceCollection AddOutpointAdapterInjections(this IServiceCollection services, IConfiguration configuration, string currentEnvironment)
        {
            AddPollyInjections(services, configuration);
            AddRabbitMqInjections(services, configuration);
            ConfigureRedisCache(services, configuration);

            if (currentEnvironment == "Local")
            {
                string ConnectionActive = configuration.GetSection("Database:Active").Value
                    ?? throw new ArgumentNullException("No se pudo obtener el valor de la base de datos que está activa");

                if (string.Equals("SQL", ConnectionActive, StringComparison.InvariantCultureIgnoreCase))
                {
                    ConfigureSqlServerDatabase(services, configuration);
                }
                else
                {
                    var liteSection = configuration.GetSection("Database:LiteDb");
                    services.Configure<LiteDbConfig>(opts =>
                    {
                        if (liteSection.Exists())
                        {
                            opts.Path = liteSection["Path"];
                        }
                    });
                    services.AddScoped<InstrumentLiteDbContext>();
                    services.AddScoped<IInstrumentsRepositoryPort, LiteInstrumentRepositoryAdapter>();
                    services.AddScoped<IUnitOfWork, InstrumentLiteDbUnitOfWork>();
                }
            }
            else
            {
                ConfigureSqlServerDatabase(services, configuration);
            }

            var assembly = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(cfg => { }, assembly);

            return services;
        }

        private static void ConfigureSqlServerDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, InstrumentSqlServerUnitOfWork>();
            var connectionString = configuration.GetSection("Database:SQL:ConnectionStrings:SqlConnection").Value
                ?? throw new ArgumentNullException("Error al obtener cadena de conexión");

            services.AddDbContext<InstrumentSqlServerDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<SqlServerInstrumentsRepositoryAdapter>();

            services.AddScoped<IInstrumentsRepositoryPort>(provider =>
            {
                var sqlAdapter = provider.GetRequiredService<SqlServerInstrumentsRepositoryAdapter>();
                var cachePort = provider.GetRequiredService<ICachePort>();
                var logger = provider.GetRequiredService<ILogger<RedisCachedInstrumentRepositoryAdapter>>();

                return new RedisCachedInstrumentRepositoryAdapter(sqlAdapter, cachePort, logger);
            });
        }

        private static void ConfigureRedisCache(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisConfig>(configuration.GetSection("Redis"));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = configuration.GetSection("Redis").Get<RedisConfig>() ?? throw new ArgumentNullException("Error al obtener la configuración de Redis");
                var logger = sp.GetRequiredService<ILogger<PollyLogger>>();

                try
                {
                    var redisOptions = ConfigurationOptions.Parse(config.ConnectionString);
                    redisOptions.ConnectTimeout = config.ConnectTimeoutMilliseconds;
                    redisOptions.SyncTimeout = config.SyncTimeoutMilliseconds;
                    redisOptions.AbortOnConnectFail = false;
                    redisOptions.AllowAdmin = false;

                    return ConnectionMultiplexer.Connect(redisOptions);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "No se pudo conectar a Redis. La cache estará deshabilitada.");
                    return null;
                }
            });

            services.AddStackExchangeRedisCache(options =>
            {
                var config = configuration.GetSection("Redis").Get<RedisConfig>() ?? throw new ArgumentNullException("Error al obtener la configuración de Redis");
                options.Configuration = config.ConnectionString;
                options.InstanceName = "MyApp_";
            });

            services.AddScoped<ICachePort>(sp =>
            {
                var redis = sp.GetRequiredService<IConnectionMultiplexer>();
                var logger = sp.GetRequiredService<ILogger<ResilientCacheAdapter>>();
                var cacheOptions = sp.GetRequiredService<IOptions<RedisConfig>>();

                if (redis == null)
                {
                    return new NullCacheAdapter();
                }

                var redisCache = new RedisCacheAdapter(redis, cacheOptions);
                return new ResilientCacheAdapter(redis, redisCache, logger, timeoutMs: 1000);
            });
        }

        public static void AddPollyInjections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IAsyncPolicy>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<PollyLogger>>();

                int eventsAllowed = int.Parse(configuration.GetSection("CircuitBreaker:EventsAllowedBeforeBreaking").Value ?? "3");
                int durationInSeconds = int.Parse(configuration.GetSection("CircuitBreaker:DurationOfBreakInSeconds").Value ?? "3");

                return Policy
                    .Handle<Exception>()
                    .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: eventsAllowed,
                        durationOfBreak: TimeSpan.FromSeconds(durationInSeconds),
                        onBreak: (ex, ts) => logger.LogError("Circuit Broken"),
                        onReset: () => logger.LogInformation("Circuit Reset")
                    );
            });
        }

        private static void AddRabbitMqInjections(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqConfig>(configuration.GetSection("RabbitMQ"));

            services.AddSingleton<IConnectionFactory>(sp =>
            {
                var rabbitConfig = configuration.GetSection("RabbitMQ").Get<RabbitMqConfig>()
                    ?? throw new ArgumentNullException("Error al obtener la configuración de RabbitMQ");

                return new ConnectionFactory
                {
                    HostName = rabbitConfig.Host,
                    Port = rabbitConfig.Port,
                    UserName = rabbitConfig.Username,
                    Password = rabbitConfig.Password
                };
            });

            services.AddSingleton<IConnection>(sp =>
            {
                var factory = sp.GetRequiredService<IConnectionFactory>();

                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });

            services.AddScoped<IMessagePublisherPort, RabbitMqPublisherAdapter>();
        }
    }

    public class PollyLogger()
    {

    }
}
