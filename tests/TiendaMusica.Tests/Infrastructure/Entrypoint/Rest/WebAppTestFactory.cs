using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace TiendaMusica.Tests.Infrastructure.Entrypoint.Rest
{
    internal class WebAppTestFactory: WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Opcional: Personaliza la configuración del host de prueba aquí
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            return base.CreateHost(builder);
        }
    }
}
