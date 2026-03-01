using TiendaMusica.Application.Injections;
using TiendaMusica.Infrastructure.OutpointAdapter.Injections;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

#region Inyecciones de servicios
builder.Services.AddApplicationInjections();
builder.Services.AddOutpointAdapterInjections();
#endregion


app.Run();
