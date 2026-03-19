using Microsoft.Extensions.DependencyInjection;
using TiendaMusica.Application.UseCases.Instruments;

namespace TiendaMusica.Application.Injections
{
    public static class ApplicationInjections
    {
        public static IServiceCollection AddApplicationInjections(this IServiceCollection services)
        {
            services.AddScoped<IInstrumentUseCase, InstrumentUseCase>();
            return services;
        }
    }
}
