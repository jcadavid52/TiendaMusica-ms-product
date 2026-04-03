using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Utilities;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Validators;

namespace TiendaMusica.Infrastructure.Entrypoint.Injections
{
    public static class RestInjections
    {
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();
        public static IServiceCollection AddRestInjections(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            services.AddAutoMapper(cfg => { }, assembly);
            services.AddHealthChecks();
            AddSwaggerInjections(services);
            services.AddScoped<IRestTools, RestTools>();
            FluentValidationInjections(services);
            AddRateLimitingInjections(services);

            return services;
        }

        private static void FluentValidationInjections(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<InstrumentRequestValidator>();
        }

        private static IServiceCollection AddSwaggerInjections(this IServiceCollection services)
        {
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

        private static IServiceCollection AddRateLimitingInjections(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? retryAfter.TotalSeconds
                        : 60;

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "Demasiadas solicitudes. Por favor, intente más tarde.",
                        retryAfter = retryAfterSeconds
                    }, cancellationToken);
                };

                options.AddPolicy("fixed", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 10
                        }));

                options.AddPolicy("sliding", httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 50,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 5,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5
                        }));

                options.AddPolicy("write", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 2
                        }));

                options.AddPolicy("read", httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 20
                        }));
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
