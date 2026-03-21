using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Config;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer;

namespace TiendaMusica.Tests.Infrastructure.Entrypoint.Rest
{
    public class WebAppTestFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context,services) =>
            {
                var activeDb = context.Configuration["Database:Active"];
                if(string.Equals("LiteDb", activeDb, StringComparison.InvariantCultureIgnoreCase))
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IOptions<LiteDbConfig>));
                    if (descriptor != null) services.Remove(descriptor);
                    var testConfig = new LiteDbConfig { Path = ":memory:" };
                    services.AddSingleton(Options.Create(testConfig));
                }
                else
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(InstrumentSqlServerDbContext));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<InstrumentSqlServerDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}");
                    });
                }
                
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            return base.CreateHost(builder);
        }
    }
}