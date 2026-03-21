using TiendaMusica.Application.Injections;
using TiendaMusica.Domain.Injections;
using TiendaMusica.Infrastructure.Entrypoint.Cli;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Constants;
using TiendaMusica.Infrastructure.Entrypoint.Injections;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Middleware;
using TiendaMusica.Infrastructure.OutpointAdapter.Injections;

//entrada del programa, se encarga de decidir si se ejecuta en modo cli o rest dependiendo de los argumentos que se le pasen
if (args.Length > 0 && InstrumentParameters.parameters.Contains(args[0].ToLower()))
{
    var cliBuilder = Host.CreateApplicationBuilder(args);
    #region Inyecciones de servicios
    cliBuilder.Services.AddDomainInjections();
    cliBuilder.Services.AddApplicationInjections();
    cliBuilder.Services.AddOutpointAdapterInjections(cliBuilder.Configuration, cliBuilder.Environment.EnvironmentName);
    cliBuilder.Services.AddCliInjections();
    #endregion

    using IHost host = cliBuilder.Build();
    await MainExecute.ExecuteAsync(args, host.Services);
    return;
}

var builder = WebApplication.CreateBuilder(args);

#region Inyecciones de servicios
builder.Services.AddDomainInjections();
builder.Services.AddApplicationInjections();
builder.Services.AddOutpointAdapterInjections(builder.Configuration, builder.Environment.EnvironmentName);
builder.Services.AddRestInjections(builder.Configuration);
#endregion

var app = builder.Build();
var env = builder.Environment.EnvironmentName;
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwaggerExtension(env);
app.MapControllers();
app.Run();

public partial class Program { }