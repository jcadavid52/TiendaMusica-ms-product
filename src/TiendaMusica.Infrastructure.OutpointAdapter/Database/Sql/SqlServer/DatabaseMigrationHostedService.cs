using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer
{
    internal class DatabaseMigrationHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseMigrationHostedService> _logger;

        public DatabaseMigrationHostedService(IServiceProvider serviceProvider, ILogger<DatabaseMigrationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InstrumentSqlServerDbContext>();

            if (!db.Database.IsRelational()) return;

            _logger.LogInformation("Iniciando proceso de migración de SQL Server con Polly...");

            var migrationPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<SqlException>(ex => ex.Number == 1801 || ex.Number == 2714),

                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 5,
                    Delay = TimeSpan.FromSeconds(2),

                    OnRetry = args =>
                    {
                        _logger.LogWarning("Conflicto temporal en la BD (Intento {Intento} de {Max}). Esperando {Tiempo}s para reintentar...",
                            args.AttemptNumber + 1,
                            5,
                            args.RetryDelay.TotalSeconds);

                        return default;
                    }
                })
                .Build();

            try
            {
                await migrationPipeline.ExecuteAsync(async token =>
                {
                    await db.Database.MigrateAsync(token);
                }, cancellationToken);

                _logger.LogInformation("Migraciones de SQL Server aplicadas exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Polly agotó los reintentos o falló debido a un error no transitorio.");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}