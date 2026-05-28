using AutoMapper;
using LiteDB;
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

        public Task<Results<IList<Instrument>>> GetAllAsync(InstrumentGetAllQueryParametersDto? queryParameters)
        {
            var results = new Results<IList<Instrument>>();
            var query = _context.InstrumentsCollection.FindAll();

            if (queryParameters == null)
            {
                results.Result = query.Select(x => _mapper.Map<Instrument>(x)).ToList();
                return Task.FromResult(results);
            }

            if (!string.IsNullOrWhiteSpace(queryParameters.Search))
            {
                var search = queryParameters.Search.ToLower();
                query = query.Where(i => i.Name.ToLower().Contains(search)
                                      || i.Description.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(queryParameters.OrderBy))
            {
                query = queryParameters.OrderBy.ToLower() switch
                {
                    "name" => queryParameters.SortDirection == SortDirection.Asc ? query.OrderBy(i => i.Name) : query.OrderByDescending(i => i.Name),
                    "price" => queryParameters.SortDirection == SortDirection.Asc ? query.OrderBy(i => i.Price) : query.OrderByDescending(i => i.Price),
                    "stock" => queryParameters.SortDirection == SortDirection.Asc ? query.OrderBy(i => i.Stock) : query.OrderByDescending(i => i.Stock),
                    "type" => queryParameters.SortDirection == SortDirection.Asc ? query.OrderBy(i => i.Type) : query.OrderByDescending(i => i.Type),
                    "creationdateutc" => queryParameters.SortDirection == SortDirection.Asc ? query.OrderBy(i => i.CreationDateUtc) : query.OrderByDescending(i => i.CreationDateUtc),
                    _ => query.OrderBy(i => i.Id)
                };
            }

            if (queryParameters.PageNumber > 0 && queryParameters.PageSize > 0)
            {
                var skip = (queryParameters.PageNumber.Value - 1) * queryParameters.PageSize.Value;
                query = query.Skip(skip).Take(queryParameters.PageSize.Value);
            }

            results.Result = query.Select(x => _mapper.Map<Instrument>(x)).ToList();
            return Task.FromResult(results);
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

        public async Task<Results<Instrument?>> GetByNameAsync(string name)
        {
            var collection = _context.InstrumentsCollection;
            var document = collection.FindOne(instrument => instrument.Name == name);
            var instrument = _mapper.Map<Instrument>(document);

            return new Results<Instrument?> { Result = instrument };
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

            if (doc.CategoryId > 0)
                doc.Category = _context.CategoriesCollection.FindById(doc.CategoryId);

            if (string.IsNullOrWhiteSpace(doc.Id))
            {
                doc.Id = Guid.NewGuid().ToString();
            }

            collection.Insert(doc);

            _context.RegisterEntity(instrument);
            return new Results<Instrument> { Result = _mapper.Map<Instrument>(doc) };
        }

        public void Update(Instrument instrument)
        {
            var collection = _context.InstrumentsCollection;
            var instrumentDocument = _mapper.Map<InstrumentDocument>(instrument);

            if (instrumentDocument.CategoryId > 0)
                instrumentDocument.Category = _context.CategoriesCollection.FindById(instrumentDocument.CategoryId);

            collection.Update(instrumentDocument);
        }

        public void DeleteMultiple(IList<Instrument> instruments)
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
