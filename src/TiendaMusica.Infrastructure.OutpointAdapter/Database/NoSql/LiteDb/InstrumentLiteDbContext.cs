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
                Context = new LiteDatabase(configs.Value.Path);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public ILiteCollection<InstrumentDocument> InstrumentsCollection => Context.GetCollection<InstrumentDocument>("Instruments");
    }
}
