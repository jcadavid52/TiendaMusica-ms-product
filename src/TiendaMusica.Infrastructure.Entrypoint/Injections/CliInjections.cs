using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Commands;

namespace TiendaMusica.Infrastructure.Entrypoint.Injections
{
    public static class CliInjections
    {
        public static IServiceCollection AddCliInjections(this IServiceCollection services)
        {
            services.AddScoped<InstrumentsCommand>();
            var assembly = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(cfg => { }, assembly);
            return services;
        }
    }
}
