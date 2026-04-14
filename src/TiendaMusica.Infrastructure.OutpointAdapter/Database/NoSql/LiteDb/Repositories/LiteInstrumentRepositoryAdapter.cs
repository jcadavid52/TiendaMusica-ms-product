using AutoMapper;
using LiteDB;
using System.Linq.Expressions;
using TiendaMusica.Domain.Dtos;
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

        public async Task<Results<IList<Instrument>>> GetAllAsync(
            SortDirection sortDirection = SortDirection.Desc,
            Expression<Func<Instrument, bool>>[]? filters = null,
            int? skip = null,
            int? take = null
            )
        {
            var collection = _context.InstrumentsCollection;
            var query = collection.FindAll();
            var domainItems = query.Select(x => _mapper.Map<Instrument>(x));

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    domainItems = domainItems.Where(filter.Compile());
                }
            }

            if (sortDirection == SortDirection.Desc)
                domainItems = domainItems.OrderByDescending(x => x.CreationDateUtc);
            else
                domainItems = domainItems.OrderBy(x => x.CreationDateUtc);

            if (skip.HasValue)
                domainItems = domainItems.Skip(skip.Value);

            if (take.HasValue)
                domainItems = domainItems.Take(take.Value);

            return new Results<IList<Instrument>> { Result = domainItems.ToList() };
        }

        public async Task<Results<Instrument?>> GetByNameAsync(string name)
        {
            var collection = _context.InstrumentsCollection;
            var document = collection.FindOne(instrument => instrument.Name == name);
            var instrument = _mapper.Map<Instrument>(document);

            return new Results<Instrument?> { Result = instrument };
        }

        public async Task<Results<Instrument?>> GetByIdAsync(string id)
        {
            var collection = _context.InstrumentsCollection;
            var document = collection.FindOne(instrument => instrument.Id == id);
            var instrument = _mapper.Map<Instrument>(document);

            return new Results<Instrument?> { Result = instrument };
        }

        public async Task<Results<IList<Instrument>>> GetByIdsAsync(IList<string> instrumentIds)
        {
            var collection = _context.InstrumentsCollection;
            var documents = collection.Find(instrument => instrumentIds.Contains(instrument.Id)).ToList();
            var instruments = documents.Select(doc => _mapper.Map<Instrument>(doc)).ToList();
            return new Results<IList<Instrument>> { Result = instruments };
        }

        public async Task<Results<int>> GetStockByType(InstrumentType type)
        {
            var collection = _context.InstrumentsCollection;
            var documents = collection.Find(instrument => instrument.Type == type).ToList();
            var currentStock = documents.Sum(instrument => instrument.Stock);

            return new Results<int> { Result = currentStock };
        }

        public async Task<Results<IList<InstrumentStockSummary>>> GetStockSummaryByInstrumentTypesAsync(IList<string> instrumentIds)
        {
            var results = new Results<IList<InstrumentStockSummary>>();
            var collection = _context.InstrumentsCollection;

            var typesToQuery = collection.Find(x => instrumentIds.Contains(x.Id))
                                         .Select(x => x.Type)
                                         .Distinct()
                                         .ToList();

            var resultado = collection.Find(x => typesToQuery.Contains(x.Type))
                .GroupBy(i => i.Type)
                .Select(g => new InstrumentStockSummary(
                    g.Key,
                    g.Sum(i => i.Stock)
                ))
                .ToList();

            results.Result = resultado;
            return results;
        }

        public async Task<Results<Instrument>> CreateAsync(Instrument instrument)
        {

            var collection = _context.InstrumentsCollection;
            var doc = _mapper.Map<InstrumentDocument>(instrument);

            if (string.IsNullOrWhiteSpace(doc.Id))
            {
                doc.Id = Guid.NewGuid().ToString();
            }

            doc.CreationDateUtc = DateTime.UtcNow;
            collection.Insert(doc);

            _context.RegisterEntity(instrument);
            return new Results<Instrument> { Result = _mapper.Map<Instrument>(doc) };
        }

        public void Update(Instrument instrument)
        {
            var collection = _context.InstrumentsCollection;
            var instrumentDocument = _mapper.Map<InstrumentDocument>(instrument);

            collection.Update(instrumentDocument);
        }

        public void DeleteMultipleAsync(IList<Instrument> instruments)
        {
            var collection = _context.InstrumentsCollection;

            var toDelete = _mapper.Map<List<InstrumentDocument>>(instruments);

            foreach (var instrument in toDelete)
            {
                collection.DeleteMany(i => i.Id == instrument.Id);
            }
        }
    }
}
