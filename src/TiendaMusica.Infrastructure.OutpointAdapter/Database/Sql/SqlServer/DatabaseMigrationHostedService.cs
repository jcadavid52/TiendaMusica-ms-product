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

            if (db.Database.IsRelational())
            {
                _logger.LogInformation("Aplicando migraciones pendientes de SQL Server...");

                try
                {
                    await db.Database.MigrateAsync(cancellationToken);
                }
                catch (SqlException ex) when (ex.Number == 1801)
                {
                    _logger.LogWarning(ex, "La base de datos ya existía. Aplicando migraciones pendientes...");
                    await db.Database.MigrateAsync(cancellationToken);
                }

                _logger.LogInformation("Migraciones de SQL Server aplicadas exitosamente.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}