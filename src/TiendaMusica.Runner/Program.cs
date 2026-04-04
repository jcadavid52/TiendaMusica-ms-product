using Serilog;
using TiendaMusica.Application.Injections;
using TiendaMusica.Domain.Injections;
using TiendaMusica.Infrastructure.Entrypoint.Cli;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Parameters;
using TiendaMusica.Infrastructure.Entrypoint.Injections;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Middleware;
using TiendaMusica.Infrastructure.OutpointAdapter.Injections;
using TiendaMusica.Utilities.Injections;

try
{
    //entrada del programa, se encarga de decidir si se ejecuta en modo cli o rest dependiendo de los argumentos que se le pasen
    if (args.Length > 0 && InstrumentParameters.parameters.Contains(args[0].ToLower()))
    {
        var cliBuilder = Host.CreateApplicationBuilder(args);

        #region Aplicar observabilidad en cli

        cliBuilder.Logging.ClearProviders();
        cliBuilder.Services.AddSerilog();

        cliBuilder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(string.Format("appsettings.{0}.json", cliBuilder.Environment.EnvironmentName), optional: false, reloadOnChange: false)
            .AddEnvironmentVariables();
        #endregion

        #region Inyecciones de servicios cli
        cliBuilder.Services.AddDomainInjections();
        cliBuilder.Services.AddApplicationInjections();
        cliBuilder.Services.AddOutpointAdapterInjections(cliBuilder.Configuration, cliBuilder.Environment.EnvironmentName);
        cliBuilder.Services.AddCliInjections();
        cliBuilder.Services.AddUtilitiesInjections();
        #endregion

        using IHost host = cliBuilder.Build();
        await MainExecute.ExecuteAsync(args, host.Services);
        return;
    }

    var builder = WebApplication.CreateBuilder(args);

    #region Aplicar observabilidad en rest
    builder.Host.UseSerilog();

    builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(string.Format("appsettings.{0}.json", builder.Environment.EnvironmentName), optional: false, reloadOnChange: false)
    .AddEnvironmentVariables();
    #endregion

    #region Inyecciones de servicios rest
    builder.Services.AddDomainInjections();
    builder.Services.AddApplicationInjections();
    builder.Services.AddOutpointAdapterInjections(builder.Configuration, builder.Environment.EnvironmentName);
    builder.Services.AddUtilitiesInjections();
    builder.Services.AddRestInjections(builder.Configuration);
    #endregion

    var app = builder.Build();
    var env = builder.Environment.EnvironmentName;
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSwaggerExtension(env);
    app.UseRateLimiter();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "El microservicio falló al iniciar");
}

public partial class Program { }