using Microsoft.Extensions.DependencyInjection;

namespace TiendaMusica.Utilities.Injections
{
    public static class UtilitiesInjections
    {
        public static IServiceCollection AddUtilitiesInjections(this IServiceCollection services)
        {
            services.AddSingleton<ITools, Tools>();

            return services;
        }
    }
}
