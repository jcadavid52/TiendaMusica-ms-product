using TiendaMusica.Application.Injections;
using TiendaMusica.Infrastructure.Entrypoint.Injections;
using TiendaMusica.Infrastructure.OutpointAdapter.Injections;

var builder = WebApplication.CreateBuilder(args);
#region Inyecciones de servicios
builder.Services.AddApplicationInjections();
builder.Services.AddOutpointAdapterInjections(builder.Configuration, builder.Environment.EnvironmentName);
builder.Services.AddEntrypointInjections(builder.Configuration);
#endregion

var app = builder.Build();
var env = builder.Environment.EnvironmentName;
if (string.Equals("local", env, StringComparison.InvariantCultureIgnoreCase) ||
    string.Equals("Development", env, StringComparison.InvariantCultureIgnoreCase) ||
    string.Equals("qa", env, StringComparison.InvariantCultureIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI(x =>
    {
        x.SwaggerEndpoint("/swagger/v1/swagger.json", "ms-instrument");
        x.DefaultModelsExpandDepth(-1);
        x.RoutePrefix = string.Empty;
    });
}
app.MapControllers();
app.Run();
