using AutoMapper;
using TiendaMusica.Domain.Models;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Mappers
{
    internal class RestMappingProfile:Profile
    {
        public RestMappingProfile()
        {
            CreateMap<Instrument, InstrumentResponse>();
            CreateMap<InstrumentRequest,Instrument>();
        }
    }
}
