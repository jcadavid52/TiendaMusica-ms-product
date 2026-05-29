using Microsoft.Extensions.Logging;
using Moq;
using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Repositories;

namespace TiendaMusica.Tests.Infrastructure.OutpointAdapter.Database.NoSql.Redis.Repositories
{
    public class RedisCachedInstrumentRepositoryAdapter_UTest
    {
        private readonly Mock<IInstrumentsRepositoryPort> _innerRepoMock;
        private readonly Mock<ICachePort> _cachePortMock;
        private readonly Mock<ILogger<RedisCachedInstrumentRepositoryAdapter>> _loggerMock;
        private readonly RedisCachedInstrumentRepositoryAdapter _adapter;

        public RedisCachedInstrumentRepositoryAdapter_UTest()
        {
            _innerRepoMock = new Mock<IInstrumentsRepositoryPort>();
            _cachePortMock = new Mock<ICachePort>();
            _loggerMock = new Mock<ILogger<RedisCachedInstrumentRepositoryAdapter>>();

            _adapter = new RedisCachedInstrumentRepositoryAdapter(
                _innerRepoMock.Object,
                _cachePortMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetAllAsync_WhenCacheHit_ShouldReturnCachedData()
        {
            var cachedInstruments = new List<Instrument>
            {
                Instrument.Create("cached1", "desc1", InstrumentType.Stringed, 150, 10).Result!,
                Instrument.Create("cached2", "desc2", InstrumentType.Wind, 200, 5).Result!,
            };

            _cachePortMock.Setup(c => c.GetAsync<IList<Instrument>>(It.IsAny<string>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = cachedInstruments
                });

            var result = await _adapter.GetAllAsync(null);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Result.Count);
            _innerRepoMock.Verify(r => r.GetAllAsync(It.IsAny<InstrumentGetAllQueryParametersDto?>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_WhenCacheMiss_ShouldGetFromInnerAndCache()
        {
            var instruments = new List<Instrument>
            {
                Instrument.Create("test1", "desc1", InstrumentType.Stringed, 150, 10).Result!,
            };

            _cachePortMock.Setup(c => c.GetAsync<IList<Instrument>>(It.IsAny<string>()))
                .ReturnsAsync(new Results<IList<Instrument>>());

            _innerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<InstrumentGetAllQueryParametersDto?>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = instruments
                });

            _cachePortMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IList<Instrument>>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new Results<bool> { Result = true });

            var result = await _adapter.GetAllAsync(null);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Single(result.Result);
            _innerRepoMock.Verify(r => r.GetAllAsync(null), Times.Once);
            _cachePortMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IList<Instrument>>(), null), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenCacheHasErrors_ShouldFallThroughToInner()
        {
            var instruments = new List<Instrument>
            {
                Instrument.Create("test1", "desc1", InstrumentType.Stringed, 150, 10).Result!,
            };

            _cachePortMock.Setup(c => c.GetAsync<IList<Instrument>>(It.IsAny<string>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.DATABASE_ERROR, "cache error")
                    }
                });

            _innerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<InstrumentGetAllQueryParametersDto?>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = instruments
                });

            _cachePortMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IList<Instrument>>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new Results<bool> { Result = true });

            var result = await _adapter.GetAllAsync(null);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Single(result.Result);
            _innerRepoMock.Verify(r => r.GetAllAsync(null), Times.Once);
            _cachePortMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IList<Instrument>>(), null), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenCacheSetFails_ShouldLogWarning()
        {
            var instruments = new List<Instrument>
            {
                Instrument.Create("test1", "desc1", InstrumentType.Stringed, 150, 10).Result!,
            };

            _cachePortMock.Setup(c => c.GetAsync<IList<Instrument>>(It.IsAny<string>()))
                .ReturnsAsync(new Results<IList<Instrument>>());

            _innerRepoMock.Setup(r => r.GetAllAsync(It.IsAny<InstrumentGetAllQueryParametersDto?>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = instruments
                });

            _cachePortMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IList<Instrument>>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new Results<bool>
                {
                    Result = false,
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.DATABASE_ERROR, "set error")
                    }
                });

            var result = await _adapter.GetAllAsync(null);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Single(result.Result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCacheHit_ShouldReturnCachedData()
        {
            var instrument = Instrument.Create("cached", "desc", InstrumentType.Stringed, 150, 10).Result!;

            _cachePortMock.Setup(c => c.GetAsync<Instrument>(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = instrument
                });

            var result = await _adapter.GetByIdAsync(instrument.Id);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
            Assert.Equal(instrument.Id, result.Result.Id);
            _innerRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCacheMiss_ShouldGetFromInnerAndCache()
        {
            var instrument = Instrument.Create("test", "desc", InstrumentType.Stringed, 150, 10).Result!;

            _cachePortMock.Setup(c => c.GetAsync<Instrument>(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>());

            _innerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = instrument
                });

            _cachePortMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Instrument>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new Results<bool> { Result = true });

            var result = await _adapter.GetByIdAsync(instrument.Id);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
            Assert.Equal(instrument.Id, result.Result.Id);
            _innerRepoMock.Verify(r => r.GetByIdAsync(instrument.Id), Times.Once);
            _cachePortMock.Verify(c => c.SetAsync(It.IsAny<string>(), instrument, null), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCacheHasErrors_ShouldFallThroughToInner()
        {
            var instrument = Instrument.Create("test", "desc", InstrumentType.Stringed, 150, 10).Result!;

            _cachePortMock.Setup(c => c.GetAsync<Instrument>(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.DATABASE_ERROR, "cache error")
                    }
                });

            _innerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = instrument
                });

            _cachePortMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Instrument>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new Results<bool> { Result = true });

            var result = await _adapter.GetByIdAsync(instrument.Id);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
            _innerRepoMock.Verify(r => r.GetByIdAsync(instrument.Id), Times.Once);
            _cachePortMock.Verify(c => c.SetAsync(It.IsAny<string>(), instrument, null), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenInnerReturnsNull_ShouldNotCache()
        {
            _cachePortMock.Setup(c => c.GetAsync<Instrument>(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>());

            _innerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = null
                });

            var result = await _adapter.GetByIdAsync("nonexistent");

            Assert.NotNull(result);
            Assert.Null(result.Result);
            _cachePortMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Instrument>(), It.IsAny<TimeSpan?>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCacheSetFails_ShouldLogWarning()
        {
            var instrument = Instrument.Create("test", "desc", InstrumentType.Stringed, 150, 10).Result!;

            _cachePortMock.Setup(c => c.GetAsync<Instrument>(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>());

            _innerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = instrument
                });

            _cachePortMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Instrument>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new Results<bool>
                {
                    Result = false,
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.DATABASE_ERROR, "set error")
                    }
                });

            var result = await _adapter.GetByIdAsync(instrument.Id);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetByIdsAsync_ShouldDelegateToInner()
        {
            var instruments = new List<Instrument>
            {
                Instrument.Create("test1", "desc1", InstrumentType.Stringed, 150, 10).Result!,
            };
            var ids = new List<string> { "id1" };

            _innerRepoMock.Setup(r => r.GetByIdsAsync(ids))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = instruments
                });

            var result = await _adapter.GetByIdsAsync(ids);

            Assert.NotNull(result);
            Assert.Single(result.Result);
            _innerRepoMock.Verify(r => r.GetByIdsAsync(ids), Times.Once);
            _cachePortMock.Verify(c => c.GetAsync<It.IsAnyType>(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldDelegateToInner()
        {
            var instrument = Instrument.Create("test", "desc", InstrumentType.Stringed, 150, 10).Result!;

            _innerRepoMock.Setup(r => r.GetByNameAsync("test"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = instrument
                });

            var result = await _adapter.GetByNameAsync("test");

            Assert.NotNull(result);
            Assert.NotNull(result.Result);
            _innerRepoMock.Verify(r => r.GetByNameAsync("test"), Times.Once);
        }

        [Fact]
        public async Task GetStockByType_ShouldDelegateToInner()
        {
            _innerRepoMock.Setup(r => r.GetStockByType(InstrumentType.Stringed))
                .ReturnsAsync(new Results<int> { Result = 10 });

            var result = await _adapter.GetStockByType(InstrumentType.Stringed);

            Assert.Equal(10, result.Result);
            _innerRepoMock.Verify(r => r.GetStockByType(InstrumentType.Stringed), Times.Once);
        }

        [Fact]
        public async Task GetStockSummaryByInstrumentTypesAsync_ShouldDelegateToInner()
        {
            var ids = new List<string> { "id1" };
            var summaries = new List<InstrumentStockSummary>
            {
                new InstrumentStockSummary(InstrumentType.Stringed, 10)
            };

            _innerRepoMock.Setup(r => r.GetStockSummaryByInstrumentTypesAsync(ids))
                .ReturnsAsync(new Results<IList<InstrumentStockSummary>>
                {
                    Result = summaries
                });

            var result = await _adapter.GetStockSummaryByInstrumentTypesAsync(ids);

            Assert.Single(result.Result);
            _innerRepoMock.Verify(r => r.GetStockSummaryByInstrumentTypesAsync(ids), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateInInnerAndSetCache()
        {
            var instrument = Instrument.Create("new", "desc", InstrumentType.Stringed, 150, 10).Result!;

            _innerRepoMock.Setup(r => r.CreateAsync(instrument))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = instrument
                });

            _cachePortMock.Setup(c => c.SetAsync(It.IsAny<string>(), instrument, It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new Results<bool> { Result = true });

            var result = await _adapter.CreateAsync(instrument);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            _innerRepoMock.Verify(r => r.CreateAsync(instrument), Times.Once);
            _cachePortMock.Verify(c => c.SetAsync(It.IsAny<string>(), instrument, null), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenCacheSetFails_ShouldLogWarning()
        {
            var instrument = Instrument.Create("new", "desc", InstrumentType.Stringed, 150, 10).Result!;

            _innerRepoMock.Setup(r => r.CreateAsync(instrument))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = instrument
                });

            _cachePortMock.Setup(c => c.SetAsync(It.IsAny<string>(), instrument, It.IsAny<TimeSpan?>()))
                .ReturnsAsync(new Results<bool>
                {
                    Result = false,
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.DATABASE_ERROR, "set error")
                    }
                });

            var result = await _adapter.CreateAsync(instrument);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Update_ShouldDelegateToInner()
        {
            var instrument = Instrument.Create("test", "desc", InstrumentType.Stringed, 150, 10).Result!;

            _adapter.Update(instrument);

            _innerRepoMock.Verify(r => r.Update(instrument), Times.Once);
            _cachePortMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()), Times.Never);
        }

        [Fact]
        public void DeleteMultiple_ShouldDelegateToInner()
        {
            var instruments = new List<Instrument>
            {
                Instrument.Create("test", "desc", InstrumentType.Stringed, 150, 10).Result!
            };

            _adapter.DeleteMultiple(instruments);

            _innerRepoMock.Verify(r => r.DeleteMultiple(instruments), Times.Once);
            _cachePortMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
