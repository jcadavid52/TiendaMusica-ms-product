using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Adapters;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Config;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Repositories;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer;

namespace TiendaMusica.Tests.Infrastructure.Entrypoint.Rest
{
    public class WebAppTestFactory : WebApplicationFactory<Program>,IAsyncLifetime
    {
        public string ActiveDb { get; private set; } = string.Empty;
        public HttpClient? Client { get; private set; }
        public IList<InstrumentCommonInfoDto> InitialInstruments { get; private set; } = [];

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var activeDb = context.Configuration["Database:Active"] ?? throw new ArgumentNullException("Valor de la base de datos activa el null");

                if (activeDb.Equals("LiteDb", StringComparison.OrdinalIgnoreCase))
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

                    RemoveRedisInjections(services);
                }

                RemoveRabbitMqInjections(services);

                ActiveDb = activeDb;

            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Local");
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

        private void RemoveRedisInjections(IServiceCollection services)
        {
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(IConnectionMultiplexer) ||
                d.ServiceType == typeof(ICachePort))
                .ToList();

            foreach (var d in descriptors) services.Remove(d);

            services.AddScoped<ICachePort, NullCacheAdapter>();
        }

        public async Task<IList<InstrumentCommonInfoDto>> GetAllInstrumentDatabase()
        {
            if (string.Equals("LiteDb", ActiveDb, StringComparison.InvariantCultureIgnoreCase))
            {
                using (var scope = Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<InstrumentLiteDbContext>();
                    var entities = db.InstrumentsCollection.FindAll()
                        .OrderByDescending(i => i.CreationDateUtc)
                        .ToList();

                    return entities.Select(e => new InstrumentCommonInfoDto(e.Id, e.Name, e.CreationDateUtc))
                        .ToList();
                }
            }
            else
            {
                using (var scope = Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<InstrumentSqlServerDbContext>();
                    var entities = await db.Instruments.ToListAsync();
                    return entities.Select(e => new InstrumentCommonInfoDto(e.Id, e.Name, e.CreationDateUtc))
                        .OrderByDescending(i => i.CreationDateUtc)
                        .ToList();
                }
            }
        }

        public async Task SeedInstrumentDatabaseAsync()
        {
            if (string.Equals("LiteDb", ActiveDb, StringComparison.InvariantCultureIgnoreCase))
            {
                SeedInstrumentDatabaseLiteDb();
            }
            else
            {
                await SeedCategoryDatabaseSqlServer();
                await SeedInstrumentDatabaseSqlServer();
            }
        }

        private void SeedInstrumentDatabaseLiteDb()
        {
            var instruments = BuildInstrumentDataLiteDb();
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InstrumentLiteDbContext>();

                db.InstrumentsCollection.DeleteAll();
                db.InstrumentsCollection.InsertBulk(instruments);
            }
        }

        private async Task SeedCategoryDatabaseSqlServer()
        {
            var categories = new List<Category>
            {
                new Category(1, "Instrumentos", "Instrumentos musicales"),
            };

            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InstrumentSqlServerDbContext>();
                db.Categories.RemoveRange(db.Categories);
                await db.Categories.AddRangeAsync(categories);
                await db.SaveChangesAsync();
            }
        }

        private async Task SeedInstrumentDatabaseSqlServer()
        {
            var instruments = BuildInstrumentDataMemorySql();
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InstrumentSqlServerDbContext>();
                db.Instruments.RemoveRange(db.Instruments);
                await db.Instruments.AddRangeAsync(instruments);
                await db.SaveChangesAsync();
            }
        }

        private IList<InstrumentDocument> BuildInstrumentDataLiteDb()
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
                    Stock = 1
                });
            }

            return instruments;
        }

        private IList<Instrument> BuildInstrumentDataMemorySql()
        {
            var instruments = new List<Instrument>();

            for (int i = 1; i <= 10; i++)
            {
                var instrumentResult = Instrument.Create(
                    name: $"Instrumento {i:D2}",
                    description: $"Descripción del instrumento {i}",
                    type: InstrumentType.Stringed,
                    price: 100.00m * i,
                    stock: 1,
                    categoryId: 1
                );

                instrumentResult.Result.CreationDateUtc = DateTime.UtcNow.AddMinutes(-i);

                instruments.Add(
                    instrumentResult.Result
                );
            }

            return instruments;
        }

        public async Task InitializeAsync()
        {
            Client = CreateClient();
            await SeedInstrumentDatabaseAsync();
            InitialInstruments = await GetAllInstrumentDatabase();
        }

        public async Task DisposeAsync()
        {
            Client?.Dispose();
            await Task.CompletedTask;
        }
    }

    public record InstrumentCommonInfoDto(
        string Id,
        string Name,
        DateTime CreationDateUtc
        );

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