using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Config;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Documents;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Mappers;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Repositories;

namespace TiendaMusica.Tests.Infrastructure.OutpointAdapter.Database.NoSql.LiteDb.Repositories
{
    public class LiteInstrumentRepositoryAdapter_UTest : IDisposable
    {
        private readonly IMapper _mapper;
        private readonly InstrumentLiteDbContext _context;
        private readonly LiteInstrumentRepositoryAdapter _adapter;

        public LiteInstrumentRepositoryAdapter_UTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddAutoMapper(cfg => cfg.AddProfile<LiteDBMappingProfile>());
            var serviceProvider = services.BuildServiceProvider();
            _mapper = serviceProvider.GetRequiredService<IMapper>();

            var liteDbConfig = new LiteDbConfig { Path = ":memory:" };
            var options = Options.Create(liteDbConfig);
            _context = new InstrumentLiteDbContext(options);

            _adapter = new LiteInstrumentRepositoryAdapter(_mapper, _context);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstruments_WhenRepositoryHasData()
        {
            // Arrange
            var instrument = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            await _adapter.CreateAsync(instrument);

            // Act
            var result = await _adapter.GetAllAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Single(result.Result);
            Assert.Equal("Guitarra Eléctrica", result.Result[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstruments_WhenHasPagination()
        {
            // Arrange
            var instrument1 = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            await _adapter.CreateAsync(instrument1);

            var instrument2 = Instrument.Create("Flauta", "Descripción test", InstrumentType.Wind, 300, 5).Result;
            await _adapter.CreateAsync(instrument2);

            var instrument3 = Instrument.Create("Piano", "Descripción test", InstrumentType.keyboard, 1000, 2).Result;
            await _adapter.CreateAsync(instrument3);

            int pageSize = 3;

            var query = new InstrumentGetAllQueryParametersDto(
                Search: null,
                OrderBy: null,
                PageNumber: 1,
                PageSize: pageSize);

            // Act
            var result = await _adapter.GetAllAsync(query);

            // Assert
            Assert.False(result.HasErrors);
            Assert.Equal(pageSize, result.Result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstruments_WhenHasOrderBy()
        {
            // Arrange
            var instrument1 = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            await _adapter.CreateAsync(instrument1);

            var instrument2 = Instrument.Create("Flauta", "Descripción test", InstrumentType.Wind, 300, 5).Result;
            await _adapter.CreateAsync(instrument2);

            var instrument3 = Instrument.Create("Piano", "Descripción test", InstrumentType.keyboard, 1000, 2).Result;
            await _adapter.CreateAsync(instrument3);

            int pageSize = 3;

            var query = new InstrumentGetAllQueryParametersDto(
                Search: null,
                OrderBy: "Name",
                PageNumber: 1,
                PageSize: pageSize,
                SortDirection.Asc);

            // Act
            var result = await _adapter.GetAllAsync(query);

            // Assert
            Assert.False(result.HasErrors);
            Assert.Equal(pageSize, result.Result.Count);
            Assert.Equal(instrument2.Name, result.Result[0].Name);
            Assert.Equal(instrument1.Name, result.Result[1].Name);
            Assert.Equal(instrument3.Name, result.Result[2].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstruments_WhenHasSearchTerm()
        {
            // Arrange
            var instrument1 = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            await _adapter.CreateAsync(instrument1);

            var instrument2 = Instrument.Create("Flauta", "Descripción test", InstrumentType.Wind, 300, 5).Result;
            await _adapter.CreateAsync(instrument2);

            var instrument3 = Instrument.Create("Piano", "Descripción test", InstrumentType.keyboard, 1000, 2).Result;
            await _adapter.CreateAsync(instrument3);

            int pageSize = 3;

            var query = new InstrumentGetAllQueryParametersDto(
                Search: "Guitarra",
                OrderBy: null,
                PageNumber: 1,
                PageSize: pageSize,
                SortDirection.Asc);

            // Act
            var result = await _adapter.GetAllAsync(query);

            // Assert
            Assert.False(result.HasErrors);
            Assert.Single(result.Result);
            Assert.Equal(instrument1.Name, result.Result[0].Name);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnInstrument_WhenFound()
        {
            // Arrange
            var instrument = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            await _adapter.CreateAsync(instrument);

            // Act
            var result = await _adapter.GetByNameAsync("Guitarra Eléctrica");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Equal("Guitarra Eléctrica", result.Result.Name);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnNull_WhenNotFound()
        {
            // Act
            var result = await _adapter.GetByNameAsync("NonExistentInstrument");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnInstrument_WhenFound()
        {
            // Arrange
            var instrument = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            var createdResult = await _adapter.CreateAsync(instrument);

            // Act
            var result = await _adapter.GetByIdAsync(createdResult.Result.Id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Equal(createdResult.Result.Id, result.Result.Id);
            Assert.Equal("Guitarra Eléctrica", result.Result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Act
            var result = await _adapter.GetByIdAsync("non-existent-id");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateInstrument_Successfully()
        {
            // Arrange
            var instrument = Instrument.Create("Nueva Guitarra", "Descripción test", InstrumentType.Stringed, 500, 10).Result;

            // Act
            var result = await _adapter.CreateAsync(instrument);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Equal("Nueva Guitarra", result.Result.Name);
            Assert.NotEmpty(result.Result.Id);
            Assert.NotEqual(default, result.Result.CreationDateUtc);
        }

        [Fact]
        public async Task CreateAsync_ShouldSetIdAndCreationDate_WhenIdIsEmpty()
        {
            // Arrange
            var instrument = Instrument.Create("Nueva Guitarra", "Descripción test", InstrumentType.Stringed, 500, 10).Result;

            // Act
            var result = await _adapter.CreateAsync(instrument);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.NotNull(result.Result.Id);
            Assert.NotEmpty(result.Result.Id);
            Assert.NotEqual(default, result.Result.CreationDateUtc);
        }

        [Fact]
        public async Task GetStockByType_ShouldReturnTotalStock_WhenInstrumentsExist()
        {
            // Arrange
            var instrument1 = Instrument.Create("Guitarra 1", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            var instrument2 = Instrument.Create("Guitarra 2", "Descripción test", InstrumentType.Stringed, 500, 20).Result;
            var instrument3 = Instrument.Create("Guitarra 3", "Descripción test", InstrumentType.Stringed, 500, 30).Result;

            await _adapter.CreateAsync(instrument1);
            await _adapter.CreateAsync(instrument2);
            await _adapter.CreateAsync(instrument3);

            // Act
            var result = await _adapter.GetStockByType(InstrumentType.Stringed);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.Equal(60, result.Result);
        }

        [Fact]
        public async Task GetStockByType_ShouldReturnZero_WhenNoInstrumentsOfType()
        {
            // Arrange
            var instrument = Instrument.Create("Guitarra", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            await _adapter.CreateAsync(instrument);

            // Act
            var result = await _adapter.GetStockByType(InstrumentType.Wind);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.Equal(0, result.Result);
        }

        [Fact]
        public async Task GetStockByType_ShouldOnlyCountInstrumentsOfSpecificType()
        {
            // Arrange
            var stringedInstrument = Instrument.Create("Guitarra", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            var windInstrument = Instrument.Create("Flauta", "Descripción test", InstrumentType.Wind, 300, 5).Result;
            var keyboardInstrument = Instrument.Create("Piano", "Descripción test", InstrumentType.keyboard, 1000, 2).Result;

            await _adapter.CreateAsync(stringedInstrument);
            await _adapter.CreateAsync(windInstrument);
            await _adapter.CreateAsync(keyboardInstrument);

            // Act
            var stringedResult = await _adapter.GetStockByType(InstrumentType.Stringed);
            var windResult = await _adapter.GetStockByType(InstrumentType.Wind);
            var keyboardResult = await _adapter.GetStockByType(InstrumentType.keyboard);

            // Assert
            Assert.Equal(10, stringedResult.Result);
            Assert.Equal(5, windResult.Result);
            Assert.Equal(2, keyboardResult.Result);
        }

        [Fact]
        public async Task GetStockSummaryByInstrumentTypesAsync()
        {
            // Arrange
            var instrument1 = Instrument.Create("Guitarra 1", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            var instrument2 = Instrument.Create("Guitarra 2", "Descripción test", InstrumentType.Stringed, 500, 20).Result;
            var instrument3 = Instrument.Create("Batería", "Descripción test", InstrumentType.Percussion, 500, 30).Result;
            int expectedTotalStockStringed = 30;
            int expectedTotalStockPercussion = 30;
            int expectedTypesCount = 2;

            await _adapter.CreateAsync(instrument1);
            await _adapter.CreateAsync(instrument2);
            await _adapter.CreateAsync(instrument3);

            var ids = new List<string> { instrument1.Id, instrument2.Id, instrument3.Id };

            // Act
            var result = await _adapter.GetStockSummaryByInstrumentTypesAsync(ids);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.Equal(expectedTypesCount, result.Result.Count);
            Assert.Equal(expectedTotalStockStringed, result.Result[0].TotalStock);
            Assert.Equal(expectedTotalStockPercussion, result.Result[1].TotalStock);
        }

        [Fact]
        public async Task CreateAsync_ShouldPersistInstrument_ToDatabase()
        {
            // Arrange
            var instrument = Instrument.Create("Guitarra Persistente", "Descripción test", InstrumentType.Stringed, 500, 10).Result;

            // Act
            await _adapter.CreateAsync(instrument);
            var retrievedInstrument = await _adapter.GetByNameAsync("Guitarra Persistente");

            // Assert
            Assert.NotNull(retrievedInstrument.Result);
            Assert.Equal("Guitarra Persistente", retrievedInstrument.Result.Name);
            Assert.Equal(10, retrievedInstrument.Result.Stock);
        }

        [Fact]
        public void InstrumentDocument_ShouldHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var document = new InstrumentDocument();

            // Assert
            Assert.Equal(string.Empty, document.Id);
            Assert.Equal(string.Empty, document.Name);
            Assert.Equal(string.Empty, document.Description);
            Assert.Equal(0m, document.Price);
            Assert.Equal(0, document.Stock);
        }

        [Theory]
        [InlineData(InstrumentType.Stringed)]
        [InlineData(InstrumentType.Wind)]
        [InlineData(InstrumentType.keyboard)]
        public void InstrumentDocument_ShouldSupportAllInstrumentTypes(InstrumentType type)
        {
            // Arrange & Act
            var document = new InstrumentDocument { Type = type };

            // Assert
            Assert.Equal(type, document.Type);
        }

        [Fact]
        public void InstrumentDocument_ShouldStoreAllProperties()
        {
            // Arrange & Act
            var document = new InstrumentDocument
            {
                Id = "test-id",
                Name = "Guitarra Eléctrica",
                Description = "Descripción del instrumento",
                Type = InstrumentType.Stringed,
                Price = 1500.00m,
                Stock = 5,
                CreationDateUtc = DateTime.UtcNow
            };

            // Assert
            Assert.Equal("test-id", document.Id);
            Assert.Equal("Guitarra Eléctrica", document.Name);
            Assert.Equal("Descripción del instrumento", document.Description);
            Assert.Equal(InstrumentType.Stringed, document.Type);
            Assert.Equal(1500.00m, document.Price);
            Assert.Equal(5, document.Stock);
            Assert.True(document.CreationDateUtc <= DateTime.UtcNow);
        }

        [Fact]
        public void LiteDbConfig_ShouldHaveDefaultPath()
        {
            // Arrange & Act
            var config = new LiteDbConfig();

            // Assert
            Assert.Null(config.Path);
        }

        [Fact]
        public void LiteDbConfig_ShouldStoreCustomPath()
        {
            // Arrange & Act
            var config = new LiteDbConfig { Path = "test/path.db" };

            // Assert
            Assert.Equal("test/path.db", config.Path);
        }

        [Fact]
        public async Task DeleteMultiple_ShouldDeleteInstruments_WhenSuccessful()
        {
            // Arrange
            var instrument1 = Instrument.Create("Guitarra 1", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            var instrument2 = Instrument.Create("Guitarra 2", "Descripción test", InstrumentType.Stringed, 500, 15).Result;
            var instrument3 = Instrument.Create("Guitarra 3", "Descripción test", InstrumentType.Stringed, 500, 20).Result;

            await _adapter.CreateAsync(instrument1);
            await _adapter.CreateAsync(instrument2);
            await _adapter.CreateAsync(instrument3);

            var instrumentsToDelete = new List<Instrument> { instrument1, instrument2, instrument3 };

            // Act
             _adapter.DeleteMultiple(instrumentsToDelete);

            // Assert
            var remaining = await _adapter.GetAllAsync(null);
            Assert.Empty(remaining.Result);
        }

        [Fact]
        public async Task Update_ShouldPersistChanges_ToDatabase()
        {
            // Arrange
            var instrument = Instrument.Create("Guitarra Persistente", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            await _adapter.CreateAsync(instrument);

            instrument.Update("Guitarra Modificada", "Nueva descripción", InstrumentType.Stringed);

            // Act
            _adapter.Update(instrument);

            // Verify persistence
            var retrievedInstrument = await _adapter.GetByIdAsync(instrument.Id);
            Assert.NotNull(retrievedInstrument.Result);
            Assert.Equal("Guitarra Modificada", retrievedInstrument.Result.Name);
            Assert.Equal("Nueva descripción", retrievedInstrument.Result.Description);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
