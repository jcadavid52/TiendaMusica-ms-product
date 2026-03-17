using AutoMapper;
using LiteDB;
using TiendaMusica.Application.Ports;
using TiendaMusica.Domain.Enums;
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
        public async Task<Results<IList<Instrument>>> GetAllAsync()
        {
            var result = new Results<IList<Instrument>>();

            try
            {
                result.Result = await Task.Run(() =>
                {
                    var collection = _context.InstrumentsCollection;

                    var documents = collection.FindAll().ToList();

                    return documents.Select(x => _mapper.Map<Instrument>(x)).ToList();
                });
            }
            catch (Exception ex)
            {
                result.AddError(ErrorCode.SERVER_ERROR, $"Error obteniendo instrumentos-Lite-Repository {ex}");
            }

            return result;
        }

        public async Task<Results<Instrument>> GetByNameAsync(string name)
        {
            var result = new Results<Instrument>();

            try
            {
                var collection = _context.InstrumentsCollection;
                var document = collection.FindOne(instrument => instrument.Name == name);
                result.Result = _mapper.Map<Instrument>(document);
            }
            catch (Exception ex)
            {
                result.AddError(ErrorCode.SERVER_ERROR, $"Error obteniendo instrumento por su nombre Lite-Repository: {ex.Message}");
            }

            return result;
        }

        public async Task<Results<Instrument>> CreateAsync(Instrument instrument)
        {
            var result = new Results<Instrument>();

            try
            {
                await Task.Run(() =>
                {
                    var collection = _context.InstrumentsCollection;

                    var document = _mapper.Map<InstrumentDocument>(instrument);

                    if (string.IsNullOrWhiteSpace(document.Id))
                    {
                        document.Id = Guid.NewGuid().ToString();
                    }

                    document.CreationDateUtc = DateTime.UtcNow;

                    collection.Insert(document);

                    instrument.Id = document.Id;
                    instrument.CreationDateUtc = document.CreationDateUtc;

                    result.Result = instrument;
                });
            }
            catch (Exception ex)
            {
                result.AddError(ErrorCode.SERVER_ERROR, $"Error creando instrumento-Lite-Repository: {ex.Message}");
            }

            return result;
        }

        public async Task<Results<int>> GetStockByType(InstrumentType type)
        {
            var result = new Results<int>();

            try
            {
                var collection = _context.InstrumentsCollection;
                var documents = collection.Find(instrument => instrument.Type == type).ToList();
                var currentStock = documents.Sum(instrument => instrument.Stock);
                result.Result = currentStock;
            }
            catch (Exception ex)
            {
                result.AddError(ErrorCode.SERVER_ERROR, $"Error obteniendo stock por tipo-Lite-Repository: {ex.Message}");
            }

            return result;
        }
    }
}
