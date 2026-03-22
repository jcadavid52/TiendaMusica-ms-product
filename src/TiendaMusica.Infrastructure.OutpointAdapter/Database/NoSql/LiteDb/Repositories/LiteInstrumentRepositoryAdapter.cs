using AutoMapper;
using LiteDB;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
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

        public async Task<Results<IList<Instrument>>> GetAllAsync(SortDirection sortDirection = SortDirection.Asc)
        {
            var instruments = await Task.Run(() =>
            {
                var collection = _context.InstrumentsCollection;
                IEnumerable<InstrumentDocument> documents;

                if (sortDirection == SortDirection.Desc)
                    documents = collection.FindAll().OrderByDescending(x => x.CreationDateUtc).ToList();
                else
                    documents = collection.FindAll().OrderBy(x => x.CreationDateUtc).ToList();

                return documents.Select(x => _mapper.Map<Instrument>(x)).ToList();
            });

            return new Results<IList<Instrument>> { Result = instruments };
        }

        public async Task<Results<Instrument>> GetByNameAsync(string name)
        {
            var collection = _context.InstrumentsCollection;
            var document = collection.FindOne(instrument => instrument.Name == name);
            var instrument = _mapper.Map<Instrument>(document);

            return new Results<Instrument> { Result = instrument };
        }

        public async Task<Results<Instrument>> CreateAsync(Instrument instrument)
        {
            var document = await Task.Run(() =>
            {
                var collection = _context.InstrumentsCollection;
                var doc = _mapper.Map<InstrumentDocument>(instrument);

                if (string.IsNullOrWhiteSpace(doc.Id))
                {
                    doc.Id = Guid.NewGuid().ToString();
                }

                doc.CreationDateUtc = DateTime.UtcNow;
                collection.Insert(doc);

                instrument.Id = doc.Id;
                instrument.CreationDateUtc = doc.CreationDateUtc;

                return doc;
            });

            return new Results<Instrument> { Result = instrument };
        }

        public async Task<Results<int>> GetStockByType(InstrumentType type)
        {
            var collection = _context.InstrumentsCollection;
            var documents = collection.Find(instrument => instrument.Type == type).ToList();
            var currentStock = documents.Sum(instrument => instrument.Stock);

            return new Results<int> { Result = currentStock };
        }
    }
}
