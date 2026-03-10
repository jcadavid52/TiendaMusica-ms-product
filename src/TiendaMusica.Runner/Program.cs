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
app.UseSwaggerExtension(env);
app.MapControllers();
app.Run();
public partial class Program { }