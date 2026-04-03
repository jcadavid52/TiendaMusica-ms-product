using AutoMapper;
using TiendaMusica.Domain.Models;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Mappers
{
    public class LiteDBMappingProfile : Profile
    {
        public LiteDBMappingProfile()
        {
            CreateMap<Instrument, InstrumentDocument>();
            CreateMap<InstrumentDocument, Instrument>();
        }
    }
}
