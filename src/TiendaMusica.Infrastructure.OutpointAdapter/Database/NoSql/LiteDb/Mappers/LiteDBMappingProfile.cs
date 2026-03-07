using AutoMapper;
using System.Diagnostics.Metrics;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Mappers
{
    internal class LiteDBMappingProfile:Profile
    {
        public LiteDBMappingProfile()
        {
            CreateMap<Instrument, InstrumentDocument>();
        }
    }
}
