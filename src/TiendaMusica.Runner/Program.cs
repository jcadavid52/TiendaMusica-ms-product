using TiendaMusica.Application.Injections;
using TiendaMusica.Domain.Injections;
using TiendaMusica.Infrastructure.Entrypoint.Cli;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Constants;
using TiendaMusica.Infrastructure.Entrypoint.Injections;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Middleware;
using TiendaMusica.Infrastructure.OutpointAdapter.Injections;

var builder = WebApplication.CreateBuilder(args);
#region Inyecciones de servicios
builder.Services.AddDomainInjections();
builder.Services.AddApplicationInjections();
builder.Services.AddOutpointAdapterInjections(builder.Configuration, builder.Environment.EnvironmentName);
builder.Services.AddEntrypointInjections(builder.Configuration);
#endregion
if (args.Length > 0 && InstrumentParameters.parameters.Contains(args[0].ToLower()))
{
    builder.Services.AddCliInjections();
    MainExecute.ExecuteAsync(args, builder.Services).GetAwaiter().GetResult();
}
else
{
    var app = builder.Build();
    var env = builder.Environment.EnvironmentName;
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSwaggerExtension(env);
    app.MapControllers();
    app.Run();
}
public partial class Program { }