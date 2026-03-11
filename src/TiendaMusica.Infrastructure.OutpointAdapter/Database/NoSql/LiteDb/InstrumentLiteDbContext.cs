using LiteDB;
using Microsoft.Extensions.Options;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Config;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb
{
    public class InstrumentLiteDbContext
    {
        public LiteDatabase Context { get; }

        public InstrumentLiteDbContext(IOptions<LiteDbConfig> configs)
        {
            try
            {
                var configuredPath = configs?.Value?.Path;
                if (string.IsNullOrWhiteSpace(configuredPath))
                {
                    configuredPath = "LocalDatabase/litedb.db";
                }

                var dbPath = Path.IsPathRooted(configuredPath)
                    ? configuredPath
                    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));

                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                Context = new LiteDatabase(dbPath);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public ILiteCollection<InstrumentDocument> InstrumentsCollection => Context.GetCollection<InstrumentDocument>("Instruments");
    }
}
