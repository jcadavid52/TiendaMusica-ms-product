using AutoMapper;
using TiendaMusica.Domain.Models;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Mappers
{
    public class LiteDBMappingProfile : Profile
    {
        public LiteDBMappingProfile()
        {
            CreateMap<Instrument, InstrumentDocument>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));

            CreateMap<InstrumentDocument, Instrument>()
                .ConvertUsing(new InstrumentDocumentToInstrumentConverter());

            CreateMap<Category, CategoryDocument>();
            CreateMap<CategoryDocument, Category>()
                .ConstructUsing(src => new Category(src.Id, src.Name, src.Description));
        }
    }
}
