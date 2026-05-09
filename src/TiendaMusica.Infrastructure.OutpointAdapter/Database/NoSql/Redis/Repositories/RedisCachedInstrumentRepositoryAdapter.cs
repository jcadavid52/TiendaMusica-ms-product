using Microsoft.Extensions.Logging;
using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Repositories
{
    public class RedisCachedInstrumentRepositoryAdapter : IInstrumentsRepositoryPort
    {
        private readonly IInstrumentsRepositoryPort _innerRepository;
        private readonly ICachePort _cachePort;
        private readonly ILogger<RedisCachedInstrumentRepositoryAdapter> _logger;
        private const string CACHE_KEY_PREFIX = "product:instrument:";
        private const string CACHE_KEY_LIST_PREFIX = "product:instruments:";

        public RedisCachedInstrumentRepositoryAdapter(
            IInstrumentsRepositoryPort innerRepository,
            ICachePort cachePort,
            ILogger<RedisCachedInstrumentRepositoryAdapter> logger)
        {
            _innerRepository = innerRepository;
            _cachePort = cachePort;
            _logger = logger;
        }

        public async Task<Results<IList<Instrument>>> GetAllAsync(InstrumentGetAllQueryParametersDto? queryParameters)
        {
            string cacheKey = $"{CACHE_KEY_LIST_PREFIX}" +
                $"paged:{queryParameters?.PageNumber}:pageSize:{queryParameters?.PageSize}:search:{queryParameters?.Search}:" +
                $"orderBy:{queryParameters?.OrderBy}:sort:{queryParameters?.SortDirection}";

            var cachedListResult = await _cachePort.GetAsync<IList<Instrument>>(cacheKey);

            if (cachedListResult.Result != null && cachedListResult.IsSuccess)
            {
                return new Results<IList<Instrument>> { Result = cachedListResult.Result };
            }

            if (cachedListResult.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores al obtener la lista de instrumentos desde la cache con clave {CacheKey}: {Errors}", cacheKey, cachedListResult.Errors);
            }

            var result = await _innerRepository.GetAllAsync(queryParameters);
            if (result.Result != null && result.IsSuccess)
            {
                var setResult = await _cachePort.SetAsync(cacheKey, result.Result);
                if (!setResult.Result)
                {
                    _logger.LogWarning("Se encontraron errores al guardar la lista de instrumentos en la cache con clave {CacheKey}: {Errors}", cacheKey, setResult.Errors);
                }
            }

            return result;
        }

        public async Task<Results<Instrument?>> GetByIdAsync(string id)
        {
            string cacheKey = $"{CACHE_KEY_PREFIX}{id}";
            var cachedResult = await _cachePort.GetAsync<Instrument>(cacheKey);
            if (cachedResult.Result != null && cachedResult.IsSuccess)
            {
                return new Results<Instrument?> { Result = cachedResult.Result };
            }

            if (cachedResult.HasErrors)
            {
                _logger.LogWarning("Se encontraron errores al obtener el instrumento con id {Id} desde la cache con clave {CacheKey}: {Errors}", id, cacheKey, cachedResult.Errors);
            }

            var findResult = await _innerRepository.GetByIdAsync(id);
            if (findResult.Result != null)
            {
                var setResult = await _cachePort.SetAsync(cacheKey, findResult.Result);

                if (!setResult.Result)
                {
                    _logger.LogWarning("Se encontraron errores al guardar el instrumento con id {Id} en la cache con clave {CacheKey}: {Errors}", id, cacheKey, setResult.Errors);
                }
            }

            return findResult;
        }

        public async Task<Results<IList<Instrument>>> GetByIdsAsync(IList<string> instrumentIds)
        {
            return await _innerRepository.GetByIdsAsync(instrumentIds);
        }

        public async Task<Results<Instrument?>> GetByNameAsync(string name)
        {
            return await _innerRepository.GetByNameAsync(name);
        }

        public async Task<Results<int>> GetStockByType(InstrumentType type)
        {
            return await _innerRepository.GetStockByType(type);
        }

        public async Task<Results<IList<InstrumentStockSummary>>> GetStockSummaryByInstrumentTypesAsync(IList<string> instrumentIds)
        {
            return await _innerRepository.GetStockSummaryByInstrumentTypesAsync(instrumentIds); ;
        }

        public async Task<Results<Instrument>> CreateAsync(Instrument instrument)
        {
            var createResult = await _innerRepository.CreateAsync(instrument);

            string cacheKey = $"{CACHE_KEY_PREFIX}{instrument.Id}";
            var setResult = await _cachePort.SetAsync(cacheKey, instrument);

            if (!setResult.Result)
            {
                _logger.LogWarning("Se encontraron errores al guardar el instrumento en la cache con clave {CacheKey}: {Errors}", cacheKey, setResult.Errors);
            }

            return createResult;
        }

        public void Update(Instrument instrument)
        {
            _innerRepository.Update(instrument);
        }

        public void DeleteMultiple(IList<Instrument> instruments)
        {
            _innerRepository.DeleteMultiple(instruments);
        }
    }
}
