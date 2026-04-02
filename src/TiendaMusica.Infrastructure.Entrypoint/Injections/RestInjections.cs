using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Validators;

namespace TiendaMusica.Infrastructure.Entrypoint.Injections
{
    public static class RestInjections
    {
        public static IServiceCollection AddRestInjections(this IServiceCollection services, IConfiguration configuration)
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
            AddSwaggerInjections(services);
            services.AddScoped<IRestTools, RestTools>();
            FluentValidationInjections(services);

            return services;
        }

        private static void FluentValidationInjections(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<InstrumentRequestValidator>();
        }

        public static IServiceCollection AddSwaggerInjections(this IServiceCollection services)
        {
            var assembly = Assembly.Load("TiendaMusica.Infrastructure.Entrypoint");
            services.AddSwaggerExamplesFromAssemblies(assembly);

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(swaggerConf =>
            {
                swaggerConf.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["controller"]}_{e.ActionDescriptor.RouteValues["action"]}");

                swaggerConf.SwaggerDoc("v1", new()
                {
                    Title = "Tienda Música",
                    Description = "Microservicio encargado de administrar los catálogos de distintos intrumentos musicales",
                    Version = "v1",
                    Contact = new()
                    {
                        Name = "Tienda música",
                        Email = "contact@tiendamusica.com",
                        Url = new Uri("https://www.Crystal.com.co")
                    }
                });

                swaggerConf.ExampleFilters();
                swaggerConf.EnableAnnotations();
            });

            return services;
        }
        public static void UseSwaggerExtension(this IApplicationBuilder builder, string env)
        {
            if (string.Equals("local", env, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals("Development", env, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals("qa", env, StringComparison.InvariantCultureIgnoreCase))
            {
                builder.UseSwagger();
                builder.UseSwaggerUI(x =>
                {
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "ms-instrument");
                    x.DefaultModelsExpandDepth(-1);
                    x.RoutePrefix = string.Empty;
                });
            }
        }
    }
}
