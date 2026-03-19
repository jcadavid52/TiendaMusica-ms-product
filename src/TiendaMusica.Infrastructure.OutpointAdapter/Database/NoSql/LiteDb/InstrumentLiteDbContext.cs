using LiteDB;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Config;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb
{
    [ExcludeFromCodeCoverage]
    public class InstrumentLiteDbContext : IDisposable
    {
        public LiteDatabase Context { get; }

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
                    // Lógica original para archivos físicos
                    if (string.IsNullOrWhiteSpace(configuredPath))
                        configuredPath = "LocalDatabase/litedb.db";

                    var dbPath = Path.IsPathRooted(configuredPath)
                        ? configuredPath
                        : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));

                    var directory = Path.GetDirectoryName(dbPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    // Importante: Usamos Connection=Shared para mitigar bloqueos si se usa archivo
                    Context = new LiteDatabase($"Filename={dbPath};Connection=shared");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public ILiteCollection<InstrumentDocument> InstrumentsCollection => Context.GetCollection<InstrumentDocument>("Instruments");

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}
