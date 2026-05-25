using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnEmptyList_WhenRepositoryIsEmpty()
        //{
        //    // Act
        //    var result = await _adapter.GetAllAsync();

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.HasErrors);
        //    Assert.NotNull(result.Result);
        //    Assert.Empty(result.Result);
        //}

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnInstruments_WhenRepositoryHasData()
        //{
        //    // Arrange
        //    var instrument = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
        //    await _adapter.CreateAsync(instrument);

        //    // Act
        //    var result = await _adapter.GetAllAsync();

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.HasErrors);
        //    Assert.NotNull(result.Result);
        //    Assert.Single(result.Result);
        //    Assert.Equal("Guitarra Eléctrica", result.Result[0].Name);
        //}

        [Fact]
        public async Task GetByNameAsync_ShouldReturnInstrument_WhenFound()
        {
            // Arrange
            var instrument = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 500, 10,1).Result;
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
            var instrument = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 500, 10,1).Result;
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
            var instrument = Instrument.Create("Nueva Guitarra", "Descripción test", InstrumentType.Stringed, 500, 10,1).Result;

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
            var instrument = Instrument.Create("Nueva Guitarra", "Descripción test", InstrumentType.Stringed, 500, 10,1).Result;

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
            var instrument1 = Instrument.Create("Guitarra 1", "Descripción test", InstrumentType.Stringed, 500, 10,1).Result;
            var instrument2 = Instrument.Create("Guitarra 2", "Descripción test", InstrumentType.Stringed, 500, 20,1).Result;
            var instrument3 = Instrument.Create("Guitarra 3", "Descripción test", InstrumentType.Stringed, 500, 30,1).Result;

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
            var instrument = Instrument.Create("Guitarra", "Descripción test", InstrumentType.Stringed, 500, 10,1).Result;
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
            var stringedInstrument = Instrument.Create("Guitarra", "Descripción test", InstrumentType.Stringed, 500, 10,1).Result;
            var windInstrument = Instrument.Create("Flauta", "Descripción test", InstrumentType.Wind, 300, 5,1).Result;
            var keyboardInstrument = Instrument.Create("Piano", "Descripción test", InstrumentType.keyboard, 1000, 2,1).Result;

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
        public async Task CreateAsync_ShouldPersistInstrument_ToDatabase()
        {
            // Arrange
            var instrument = Instrument.Create("Guitarra Persistente", "Descripción test", InstrumentType.Stringed, 500, 10,1).Result;

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

        //[Fact]
        //public async Task DeleteMultipleAsync_ShouldDeleteInstruments_WhenSuccessful()
        //{
        //    // Arrange
        //    var instrument1 = Instrument.Create("Guitarra 1", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
        //    var instrument2 = Instrument.Create("Guitarra 2", "Descripción test", InstrumentType.Stringed, 500, 15).Result;
        //    var instrument3 = Instrument.Create("Guitarra 3", "Descripción test", InstrumentType.Stringed, 500, 20).Result;

        //    await _adapter.CreateAsync(instrument1);
        //    await _adapter.CreateAsync(instrument2);
        //    await _adapter.CreateAsync(instrument3);

        //    var idsToDelete = new List<string> { instrument1.Id, instrument2.Id, instrument3.Id };

        //    // Act
        //    var result = await _adapter.DeleteMultipleAsync(idsToDelete);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.HasErrors);
        //    Assert.Equal(3, result.Result);

        //    // Verify deletion
        //    var remaining = await _adapter.GetAllAsync();
        //    Assert.Empty(remaining.Result);
        //}

        //[Fact]
        //public async Task DeleteMultipleAsync_ShouldReturnNotFoundError_WhenSomeIdsNotExist()
        //{
        //    // Arrange
        //    var instrument1 = Instrument.Create("Guitarra 1", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
        //    var instrument2 = Instrument.Create("Guitarra 2", "Descripción test", InstrumentType.Stringed, 500, 15).Result;

        //    await _adapter.CreateAsync(instrument1);
        //    await _adapter.CreateAsync(instrument2);

        //    var idsToDelete = new List<string> { instrument1.Id, "non-existent-id", instrument2.Id };

        //    // Act
        //    var result = await _adapter.DeleteMultipleAsync(idsToDelete);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.True(result.HasErrors);
        //    Assert.Equal(ErrorCode.NOT_FOUND, result.Errors[0].ErrorCode);
        //    Assert.Contains("non-existent-id", result.Errors[0].Message);

        //    // Verify no deletion occurred
        //    var remaining = await _adapter.GetAllAsync();
        //    Assert.Equal(2, remaining.Result.Count);
        //}

        //[Fact]
        //public async Task DeleteMultipleAsync_ShouldReturnNotFoundError_WhenAllIdsNotExist()
        //{
        //    // Arrange
        //    var instrument1 = Instrument.Create("Guitarra 1", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
        //    await _adapter.CreateAsync(instrument1);

        //    var idsToDelete = new List<string> { "non-existent-1", "non-existent-2" };

        //    // Act
        //    var result = await _adapter.DeleteMultipleAsync(idsToDelete);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.True(result.HasErrors);
        //    Assert.Equal(ErrorCode.NOT_FOUND, result.Errors[0].ErrorCode);
        //    Assert.Contains("non-existent-1", result.Errors[0].Message);
        //    Assert.Contains("non-existent-2", result.Errors[0].Message);
        //}

        //[Fact]
        //public async Task DeleteMultipleAsync_ShouldDeleteSingleInstrument()
        //{
        //    // Arrange
        //    var instrument = Instrument.Create("Guitarra Única", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
        //    await _adapter.CreateAsync(instrument);

        //    var idsToDelete = new List<string> { instrument.Id };

        //    // Act
        //    var result = await _adapter.DeleteMultipleAsync(idsToDelete);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.HasErrors);
        //    Assert.Equal(1, result.Result);

        //    // Verify deletion
        //    var remaining = await _adapter.GetAllAsync();
        //    Assert.Empty(remaining.Result);
        //}

        //[Fact]
        //public async Task UpdateAsync_ShouldUpdateInstrument_WhenSuccessful()
        //{
        //    // Arrange
        //    var instrument = Instrument.Create("Guitarra Original", "Descripción original", InstrumentType.Stringed, 500, 10).Result;
        //    await _adapter.CreateAsync(instrument);

        //    instrument.Update("Guitarra Actualizada", "Descripción actualizada", InstrumentType.Stringed);

        //    // Act
        //    await _adapter.UpdateAsync(instrument);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.HasErrors);
        //    Assert.NotNull(result.Result);
        //    Assert.Equal("Guitarra Actualizada", result.Result.Name);
        //    Assert.Equal("Descripción actualizada", result.Result.Description);
        //}

        //[Fact]
        //public async Task UpdateAsync_ShouldReturnNotFoundError_WhenInstrumentNotExists()
        //{
        //    // Arrange
        //    var instrument = Instrument.Create("Guitarra Fantasma", "Descripción test", InstrumentType.Stringed, 500, 10).Result;

        //    // Act
        //    var result = await _adapter.UpdateAsync(instrument);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.True(result.HasErrors);
        //    Assert.Equal(ErrorCode.NOT_FOUND, result.Errors[0].ErrorCode);
        //    Assert.Contains("no encontrado", result.Errors[0].Message);
        //}

        [Fact]
        public async Task UpdateAsync_ShouldPersistChanges_ToDatabase()
        {
            // Arrange
            var instrument = Instrument.Create("Guitarra Persistente", "Descripción test", InstrumentType.Stringed, 500, 10,1).Result;
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
