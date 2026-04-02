using AutoMapper;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Domain.Models;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Mappers.Resolvers;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Mappers
{
    public class RestMappingProfile : Profile
    {
        public RestMappingProfile()
        {
            CreateMap<Instrument, InstrumentResponse>()
             .ForMember(dest => dest.CreationDateUtc, opt => opt.MapFrom<DateTimeToBogotaResolver>());
            CreateMap<InstrumentRequest, CreateInstrumentCommand>();
        }
    }
}
