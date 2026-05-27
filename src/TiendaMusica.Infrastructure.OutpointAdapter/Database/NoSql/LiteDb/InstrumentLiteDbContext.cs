using LiteDB;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using TiendaMusica.Domain.Models;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Config;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb
{
    [ExcludeFromCodeCoverage]
    public class InstrumentLiteDbContext : IDisposable
    {
        public LiteDatabase Context { get; }
        private readonly List<object> _trackedEntities = new();

        public InstrumentLiteDbContext(IOptions<LiteDbConfig> configs)
        {
            try
            {
                var configuredPath = configs?.Value?.Path;
                if (configuredPath == ":memory:")
                {
                    Context = new LiteDatabase(new MemoryStream());
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(configuredPath))
                        configuredPath = "LocalDatabase/litedb.db";

                    var dbPath = Path.IsPathRooted(configuredPath)
                        ? configuredPath
                        : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));

                    var directory = Path.GetDirectoryName(dbPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    Context = new LiteDatabase($"Filename={dbPath};Connection=shared");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            SeedCategoriesIfEmpty();
        }

        private void SeedCategoriesIfEmpty()
        {
            var categories = CategoriesCollection;
            if (categories.Count() > 0) return;

            categories.Insert(new CategoryDocument { Id = 1, Name = "Cuerdas", Description = "Instrumentos de cuerda" });
            categories.Insert(new CategoryDocument { Id = 2, Name = "Viento", Description = "Instrumentos de viento" });
            categories.Insert(new CategoryDocument { Id = 3, Name = "Percusión", Description = "Instrumentos de percusión" });
            categories.Insert(new CategoryDocument { Id = 4, Name = "Teclado", Description = "Instrumentos de teclado" });
        }

        public ILiteCollection<InstrumentDocument> InstrumentsCollection => Context.GetCollection<InstrumentDocument>("Instruments");

        public ILiteCollection<CategoryDocument> CategoriesCollection => Context.GetCollection<CategoryDocument>("Categories");

        public void RegisterEntity<T>(T entity) where T : class
        {
            if (!_trackedEntities.Contains(entity))
            {
                _trackedEntities.Add(entity);
            }
        }

        public IEnumerable<AggregateRoot<TId>> GetTrackedEntities<TId>()
        {
            return _trackedEntities.OfType<AggregateRoot<TId>>();
        }

        public void ClearTracker() => _trackedEntities.Clear();

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}
