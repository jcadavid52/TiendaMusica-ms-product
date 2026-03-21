using AutoMapper;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Dtos;

namespace TiendaMusica.Infrastructure.Entrypoint.Cli.Mappers
{
    internal class CliMappingProfile:Profile
    {
        public CliMappingProfile()
        {
            CreateMap<InstrumentCreateCliRequest, CreateInstrumentCommand>();
        }
    }
}
