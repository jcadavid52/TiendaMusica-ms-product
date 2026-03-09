using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities.Examples;

namespace TiendaMusica.Infrastructure.Entrypoint.Injections
{
    public static class SwaggerInjections
    {
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

    }
}
