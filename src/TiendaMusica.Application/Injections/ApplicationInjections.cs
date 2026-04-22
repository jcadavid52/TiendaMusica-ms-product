using Microsoft.Extensions.DependencyInjection;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Application.UseCases.Validators.Instruments;
using TiendaMusica.Domain.Models;

namespace TiendaMusica.Application.Injections
{
    public static class ApplicationInjections
    {
        public static IServiceCollection AddApplicationInjections(this IServiceCollection services)
        {
            services.AddScoped<IInstrumentUseCase, InstrumentUseCase>();
            services.AddScoped<IInstrumentValidator<InstrumentUpdateCommand, Instrument>, InstrumentUpdateValidator>();
            services.AddScoped<IInstrumentValidator<InstrumentCreateCommand, bool>, InstrumentCreateValidator>();
            services.AddScoped<IInstrumentValidator<InstrumentDeleteMultipleCommand, IList<Instrument>>, InstrumentDeleteMassiveValidator>();
            return services;
        }
    }
}
