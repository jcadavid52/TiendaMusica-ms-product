using AutoMapper;
using TiendaMusica.Domain.Models;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;
using TiendaMusica.Utilities;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Mappers.Resolvers
{
    public class DateTimeToBogotaResolver : IValueResolver<Instrument, InstrumentResponse, string>
    {
        private readonly ITools _tools;

        public DateTimeToBogotaResolver(ITools tools)
        {
            _tools = tools;
        }

        public string Resolve(Instrument source, InstrumentResponse destination, string destMember, ResolutionContext context)
        {
            return _tools.DateTimeUtcToBogotaAsString(source.CreationDateUtc);
        }
    }
}
