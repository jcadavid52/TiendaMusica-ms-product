using Microsoft.Extensions.DependencyInjection;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Commands;

namespace TiendaMusica.Infrastructure.Entrypoint.Injections
{
    public static class CliInjections
    {
        public static IServiceCollection AddCliInjections(this IServiceCollection services)
        {
            services.AddScoped<InstrumentsCommand>();
            return services;
        }
    }
}
