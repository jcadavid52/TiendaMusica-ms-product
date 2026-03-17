using Microsoft.Extensions.DependencyInjection;
using TiendaMusica.Domain.Services;

namespace TiendaMusica.Domain.Injections
{
    public static class DomainInjections
    {
        public static IServiceCollection AddDomainInjections(this IServiceCollection services)
        {
            services.AddScoped<IInstrumentCreateValidationService, InstrumentCreateValidationService>();
            return services;
        }
    }
}
