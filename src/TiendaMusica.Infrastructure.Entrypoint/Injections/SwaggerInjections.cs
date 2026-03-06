using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;

namespace TiendaMusica.Infrastructure.Entrypoint.Injections
{
    public static class SwaggerInjections
    {
        public static IServiceCollection AddSwaggerInjections(this IServiceCollection services)
        {
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

                //var xmlFilename = "Crystal.Runner.xml";
                //swaggerConf.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                //swaggerConf.ExampleFilters();
                //swaggerConf.EnableAnnotations();
            });

            return services;
        }

    }
}
