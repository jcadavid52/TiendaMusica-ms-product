using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Validators;

namespace TiendaMusica.Infrastructure.Entrypoint.Injections
{
    public static class EntrypointInjections
    {
        public static IServiceCollection AddEntrypointInjections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            var assembly = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(cfg => { }, assembly);
            services.AddHealthChecks();
            services.AddSwaggerInjections();
            services.AddSingleton<IRestTools, RestTools>();
            FluentValidationInjections(services);

            return services;
        }

        private static void FluentValidationInjections(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<InstrumentRequestValidator>();
        }
    }
}
