using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System.Reflection;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Config;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Repositories;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer.Repositories;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Injections
{
    public static class OutpointAdapterInjections
    {
        public static IServiceCollection AddOutpointAdapterInjections(this IServiceCollection services, IConfiguration configuration, string currentEnvironment)
        {
            AddPollyInjections(services, configuration);

            if (currentEnvironment == "Development")
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
            var connectionString = configuration.GetSection("Database:SQL:ConnectionStrings:SqlConnection").Value
                ?? throw new ArgumentNullException("Error al obtener cadena de conexión");
            
            services.AddDbContext<InstrumentSqlServerDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<IInstrumentsRepositoryPort, SqlServerInstrumentsRepositoryAdapter>();
        }

        public static void AddPollyInjections(this IServiceCollection services, IConfiguration configuration)
        {
            int eventsAllowedBeforeBreaking = int.Parse(configuration.GetSection("CircuitBreaker:EventsAllowedBeforeBreaking").Value ?? "3");
            int durationOfBreakInSeconds = int.Parse(configuration.GetSection("CircuitBreaker:DurationOfBreakInSeconds").Value ?? "3");

            ILogger<PollyLogger> logger = services.BuildServiceProvider().GetService<ILogger<PollyLogger>>() ?? throw new ArgumentNullException("Error obteniendo servicio de logger");

            var circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: eventsAllowedBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(durationOfBreakInSeconds),
                onBreak: (exception, timespan) => { },
                onReset: () => { },
                onHalfOpen: () => { });

            services.AddSingleton<IAsyncPolicy>(circuitBreakerPolicy);
        }
    }

    public class PollyLogger()
    {

    }
}
