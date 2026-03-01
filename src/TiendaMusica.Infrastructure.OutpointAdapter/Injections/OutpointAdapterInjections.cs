using Microsoft.Extensions.DependencyInjection;
using TiendaMusica.Application.Ports;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.Sql_Server.Repositories;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Injections
{
    public static class OutpointAdapterInjections
    {
        public static IServiceCollection AddOutpointAdapterInjections(this IServiceCollection services)
        {
            services.AddSingleton<IInstrumentsRepositoryPort, SqlServerInstrumentsRepositoryAdapter>();
            return services;
        }
    }
}
