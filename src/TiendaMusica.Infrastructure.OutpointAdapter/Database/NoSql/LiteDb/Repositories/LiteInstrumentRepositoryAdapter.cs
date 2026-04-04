using AutoMapper;
using LiteDB;
using System.Linq.Expressions;
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
            var instruments = await Task.Run(() =>
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


                return domainItems.ToList();
            });

            return new Results<IList<Instrument>> { Result = instruments };
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

        public async Task<Results<int>> DeleteMultipleAsync(IList<string> instrumentIds)
        {

            var results = new Results<int>();
            var collection = _context.InstrumentsCollection;
            var count = 0;
            var toDelete = collection.Find(instrument => instrumentIds.Contains(instrument.Id)).ToList();

            if (toDelete.Count != instrumentIds.Distinct().Count())
            {
                var idsFounds = toDelete.Select(p => p.Id);
                var idsMissing = instrumentIds.Except(idsFounds);
                results.Result = count;
                return results.AddError(ErrorCode.NOT_FOUND, $"No se encontraron los registros con IDs: {string.Join(", ", idsMissing)}");
            }

            foreach (var id in instrumentIds)
            {
                var deleted = collection.DeleteMany(instrument => instrument.Id == id);
                count += deleted;
            }

            results.Result = count;
            return results;
        }
    }
}
