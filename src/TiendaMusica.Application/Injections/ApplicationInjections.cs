using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiendaMusica.Application.UseCases;

namespace TiendaMusica.Application.Injections
{
    public static class ApplicationInjections
    {
        public static IServiceCollection AddApplicationInjections(this IServiceCollection services)
        {
            services.AddSingleton<IInstrumentUseCase, InstrumentUseCase>();
            return services;
        }
    }
}
