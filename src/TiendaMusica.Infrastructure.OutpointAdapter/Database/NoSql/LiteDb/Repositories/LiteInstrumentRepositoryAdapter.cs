using AutoMapper;
using LiteDB;
using System.Linq;
using TiendaMusica.Application.Ports;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Repositories
{
    public class LiteInstrumentRepositoryAdapter : IInstrumentsRepositoryPort
    {
        private readonly IMapper _mapper;
        private readonly InstrumentLiteDbContext _context;

        public LiteInstrumentRepositoryAdapter(IMapper mapper, InstrumentLiteDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }
        public Results<IList<Instrument>> GetAll()
        {
            var result = new Results<IList<Instrument>>();

            try
            {
                var collection = _context.InstrumentsCollection;
                var documents = collection.FindAll().ToList();
                var instruments = documents.Select(x => _mapper.Map<Instrument>(x)).ToList();

                result.Result = instruments;
            }
            catch (Exception ex)
            {
                result.AddError(ErrorCode.SERVER_ERROR, $"Error obteniendo instrumentos-Lite-Repository {ex}");
            }

            return result;
        }
    }
}
