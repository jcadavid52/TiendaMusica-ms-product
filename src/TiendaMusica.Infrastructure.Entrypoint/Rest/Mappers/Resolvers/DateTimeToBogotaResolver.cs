using AutoMapper;
using TiendaMusica.Domain.Models;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;
using TiendaMusica.Utilities;

namespace TiendaMusica.Infrastructure.Entrypoint.Rest.Mappers.Resolvers
{
    internal class DateTimeToBogotaResolver : IValueResolver<Instrument, InstrumentResponse, DateTime>
    {
        private readonly ITools _tools;

        public DateTimeToBogotaResolver(ITools tools)
        {
            _tools = tools;
        }

        public DateTime Resolve(Instrument source, InstrumentResponse destination, DateTime destMember, ResolutionContext context)
        {
            return _tools.DateTimeUtcToBogota(source.CreationDateUtc);
        }
    }
}
