using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

            _logger.LogInformation("Iniciando proceso de migración de SQL Server...");

            const int maxRetries = 5;
            int delayMilliseconds = 2000; // Espera 2 segundos antes del primer reintento

            for (int retry = 1; retry <= maxRetries; retry++)
            {
                try
                {
                    // EF Core maneja la existencia de la BD de forma nativa.
                    await db.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("Migraciones de SQL Server aplicadas exitosamente.");
                    break; // Si tiene éxito, salimos del bucle
                }
                catch (SqlException ex) when (ex.Number == 1801 || ex.Number == 2714)
                {
                    // 1801 = BD ya existe, 2714 = Tabla ya existe (choques de concurrencia comunes en Docker)
                    _logger.LogWarning($"Aviso de concurrencia/existencia en BD (Intento {retry}/{maxRetries}). Esperando para reintentar...");

                    if (retry == maxRetries) throw; // Si se agotan los intentos, dejamos que falle.

                    await Task.Delay(delayMilliseconds, cancellationToken);
                    delayMilliseconds *= 2; // Duplica el tiempo de espera en cada fallo (2s, 4s, 8s...)
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico no recuperable durante la migración.");
                    throw;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}