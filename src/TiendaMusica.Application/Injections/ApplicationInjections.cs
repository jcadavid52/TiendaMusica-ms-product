using Microsoft.Extensions.DependencyInjection;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Application.UseCases.Instruments.Validators;
using TiendaMusica.Domain.Models;

namespace TiendaMusica.Application.Injections
{
    public static class ApplicationInjections
    {
        public static IServiceCollection AddApplicationInjections(this IServiceCollection services)
        {
            services.AddScoped<IInstrumentUseCase, InstrumentUseCase>();
            services.AddScoped<IGenericValidator<InstrumentUpdateCommand, Instrument>, UpdateValidator>();
            services.AddScoped<IGenericValidator<InstrumentCreateCommand, bool>, CreateValidator>();
            services.AddScoped<IGenericValidator<InstrumentDeleteMultipleCommand, IList<Instrument>>, DeleteMassiveValidator>();
            return services;
        }
    }
}
