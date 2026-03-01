using TiendaMusica.Application.Injections;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

#region Inyección de servicios
builder.Services.AddApplicationInjections();
#endregion
app.Run();
