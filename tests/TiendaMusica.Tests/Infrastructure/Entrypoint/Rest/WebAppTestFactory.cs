using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Config;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Repositories;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer;

namespace TiendaMusica.Tests.Infrastructure.Entrypoint.Rest
{
    public class WebAppTestFactory : WebApplicationFactory<Program>
    {
        public string ActiveDb { get; private set; } = string.Empty;
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var activeDb = context.Configuration["Database:Active"] ?? throw new ArgumentNullException("Valor de la base de datos activa el null");
                if (string.Equals("LiteDb", activeDb, StringComparison.InvariantCultureIgnoreCase))
                {
                    var descriptors = services.Where(d =>
                        d.ServiceType == typeof(IOptions<LiteDbConfig>) ||
                        d.ServiceType == typeof(InstrumentLiteDbContext) ||
                        d.ServiceType == typeof(IInstrumentsRepositoryPort)).ToList();

                    foreach (var d in descriptors) services.Remove(d);

                    var testConfig = new LiteDbConfig { Path = ":memory:" };
                    services.AddSingleton(Options.Create(testConfig));

                    services.AddSingleton<InstrumentLiteDbContext>();

                    services.AddSingleton<IInstrumentsRepositoryPort, LiteInstrumentRepositoryAdapter>();
                }
                else
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<InstrumentSqlServerDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<InstrumentSqlServerDbContext>(options =>
                    {
                        options.UseInMemoryDatabase($"TestDatabase_Integration");
                    });
                }

                RemoveRabbitMqInjections(services);

                ActiveDb = activeDb;

            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            return base.CreateHost(builder);
        }

        private void RemoveRabbitMqInjections(IServiceCollection services)
        {
            var rabbitConnection = services.SingleOrDefault(d => d.ServiceType == typeof(IConnection));
            if (rabbitConnection != null) services.Remove(rabbitConnection);

            var publisher = services.SingleOrDefault(d => d.ServiceType == typeof(IMessagePublisherPort));
            if (publisher != null) services.Remove(publisher);

            services.RemoveAll<IMessagePublisherPort>();
            services.AddScoped<IMessagePublisherPort, TestMessagePublisher>();
        }

        public async Task<IList<InstrumentCommonInfoDto>> ScopedDatabaseAsync()
        {
            if (string.Equals("LiteDb", ActiveDb, StringComparison.InvariantCultureIgnoreCase))
            {
                var docs = ScopedDatabaseLiteDb();

                return docs.Select(d => new InstrumentCommonInfoDto(d.Id, d.Name)).ToList();
            }
            else
            {
                var entities = await ScopedDatabaseSqlServer();
                return entities.Select(e => new InstrumentCommonInfoDto(e.Id, e.Name)).ToList();
            }
        }

        private IList<InstrumentDocument> ScopedDatabaseLiteDb()
        {
            var instruments = SeedDataLiteDb();
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InstrumentLiteDbContext>();

                db.InstrumentsCollection.DeleteAll();
                db.InstrumentsCollection.InsertBulk(instruments);
            }

            return instruments;
        }

        private async Task<IList<Instrument>> ScopedDatabaseSqlServer()
        {
            var instruments = SeedDataSqlServer();
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InstrumentSqlServerDbContext>();
                db.Instruments.RemoveRange(db.Instruments);
                await db.Instruments.AddRangeAsync(instruments);
                await db.SaveChangesAsync();
            }
            return instruments;
        }

        private IList<InstrumentDocument> SeedDataLiteDb()
        {
            var instruments = new List<InstrumentDocument>();
            for (int i = 1; i <= 10; i++)
            {
                instruments.Add(new InstrumentDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    CreationDateUtc = DateTime.UtcNow.AddMinutes(-i),
                    Name = $"Instrumento {i:D2}",
                    Description = $"Descripción del instrumento {i}",
                    Type = InstrumentType.Stringed,
                    Price = 100.00m * i,
                    Stock = i
                });
            }

            return instruments;
        }

        private IList<Instrument> SeedDataSqlServer()
        {
            var instruments = new List<Instrument>();

            for (int i = 1; i <= 10; i++)
            {
                var instrumentResult = Instrument.Create(
                    name: $"Instrumento {i:D2}",
                    description: $"Descripción del instrumento {i}",
                    type: InstrumentType.Stringed,
                    price: 100.00m * i,
                    stock: i
                );

                instrumentResult.Result.CreationDateUtc = DateTime.UtcNow.AddMinutes(-i);

                instruments.Add(
                    instrumentResult.Result
                );
            }

            return instruments;
        }
    }

    public record InstrumentCommonInfoDto(string Id, string Name);

    public class TestMessagePublisher : IMessagePublisherPort
    {
        async Task<Results<bool>> IMessagePublisherPort.PublishAsync<T>(T @event)
        {
            return new Results<bool>
            {
                Result = true,
            };
        }
    }
}