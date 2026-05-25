using Microsoft.EntityFrameworkCore;
using Polly;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer;
using TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer.Repositories;

namespace TiendaMusica.Tests.Infrastructure.OutpointAdapter.Database.Sql.Sql_Server.Repositories
{
    public class SqlServerInstrumentsRepositoryAdapter_UTest : IDisposable
    {
        private readonly InstrumentSqlServerDbContext _context;
        private readonly SqlServerInstrumentsRepositoryAdapter _adapter;

        public SqlServerInstrumentsRepositoryAdapter_UTest()
        {
            var options = new DbContextOptionsBuilder<InstrumentSqlServerDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
                .Options;
            IAsyncPolicy async = Policy.NoOpAsync();
            _context = new InstrumentSqlServerDbContext(options);
            _adapter = new SqlServerInstrumentsRepositoryAdapter(_context, async);
        }

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoDataInDatabase()
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
        //    var instrument1 = Instrument.Create("Guitarra Eléctrica", "Descripción test 1", InstrumentType.Stringed, 500, 10).Result;
        //    var instrument2 = Instrument.Create("Guitarra Acústica", "Descripción test 2", InstrumentType.Stringed, 500, 15).Result;

        //    await _adapter.CreateAsync(instrument1);
        //    await _adapter.CreateAsync(instrument2);
        //    int expectedChanges = 2;
        //    int saveResult = await SaveChangesAsync();

        //    // Act
        //    var result = await _adapter.GetAllAsync();

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.HasErrors);
        //    Assert.NotNull(result.Result);
        //    Assert.Equal(expectedChanges, result.Result.Count);
        //    Assert.Equal(expectedChanges, saveResult);
        //}

        [Fact]
        public async Task CreateAsync_ShouldCreateInstrument_Successfully()
        {
            // Arrange
            var instrument = Instrument.Create(
                "Nueva Guitarra",
                "Descripción test",
                InstrumentType.Stringed,
                500,
                10,
                1
            ).Result;
            int expectedChanges = 1;

            // Act
            var result = await _adapter.CreateAsync(instrument);
            int saveResult = await SaveChangesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Equal("Nueva Guitarra", result.Result.Name);
            Assert.NotEmpty(result.Result.Id);
            Assert.NotEqual(default, result.Result.CreationDateUtc);
            Assert.Equal(expectedChanges, saveResult);
        }

        [Fact]
        public async Task CreateAsync_ShouldPersistInstrument_ToDatabase()
        {
            // Arrange
            var instrument = Instrument.Create(
                "Guitarra Persistente",
                "Descripción test",
                InstrumentType.Stringed,
                500,
                10
                ,1
            ).Result;
            int expectedChanges = 1;

            // Act
            await _adapter.CreateAsync(instrument);
            int saveResult = await SaveChangesAsync();
            var retrievedInstrument = await _adapter.GetByNameAsync("Guitarra Persistente");

            // Assert
            Assert.NotNull(retrievedInstrument.Result);
            Assert.Equal("Guitarra Persistente", retrievedInstrument.Result.Name);
            Assert.Equal(10, retrievedInstrument.Result.Stock);
            Assert.Equal(expectedChanges, saveResult);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnInstrument_WhenFound()
        {
            // Arrange
            var instrument = Instrument.Create(
                "Guitarra Eléctrica",
                "Descripción test",
                InstrumentType.Stringed,
                500,
                10,
                1
            ).Result;
            await _adapter.CreateAsync(instrument);
            int expectedChanges = 1;
            int saveResult = await SaveChangesAsync();

            // Act
            var result = await _adapter.GetByNameAsync("Guitarra Eléctrica");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Equal("Guitarra Eléctrica", result.Result.Name);
            Assert.Equal(expectedChanges, saveResult);
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
            var instrument = Instrument.Create(
                "Guitarra Eléctrica",
                "Descripción test",
                InstrumentType.Stringed,
                500,
                10,
                1
            ).Result;

            var createdResult = await _adapter.CreateAsync(instrument);
            int expectedChanges = 1;
            int saveResult = await SaveChangesAsync();

            // Act
            var result = await _adapter.GetByIdAsync(createdResult.Result.Id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Equal(createdResult.Result.Id, result.Result.Id);
            Assert.Equal("Guitarra Eléctrica", result.Result.Name);
            Assert.Equal(expectedChanges, saveResult);
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
        public async Task GetStockByType_ShouldReturnTotalStock_WhenInstrumentsExist()
        {
            // Arrange

            var instrument1 = Instrument.Create("Guitarra 1", "Descripción test", InstrumentType.Stringed, 500, 10, 1).Result;
            var instrument2 = Instrument.Create("Guitarra 2", "Descripción test", InstrumentType.Stringed, 500, 20, 1).Result;
            var instrument3 = Instrument.Create("Guitarra 3", "Descripción test", InstrumentType.Stringed, 500, 30, 1).Result;

            await _adapter.CreateAsync(instrument1);
            await _adapter.CreateAsync(instrument2);
            await _adapter.CreateAsync(instrument3);

            int expectedChanges = 3;
            int saveResult = await SaveChangesAsync();

            // Act
            var result = await _adapter.GetStockByType(InstrumentType.Stringed);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.Equal(60, result.Result);
            Assert.Equal(expectedChanges, saveResult);
        }

        [Fact]
        public async Task GetStockByType_ShouldReturnZero_WhenNoInstrumentsOfType()
        {
            // Arrange
            var instrument = Instrument.Create(
                "Guitarra", "Descripción test",
                InstrumentType.Stringed,
                500,
                10,
                1
            ).Result;
            await _adapter.CreateAsync(instrument);
            int expectedChanges = 1;
            int saveResult = await SaveChangesAsync();

            // Act
            var result = await _adapter.GetStockByType(InstrumentType.Wind);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.Equal(0, result.Result);
            Assert.Equal(expectedChanges, saveResult);
        }

        [Fact]
        public async Task GetStockByType_ShouldOnlyCountInstrumentsOfSpecificType()
        {
            // Arrange
            var stringedInstrument = Instrument.Create("Guitarra", "Descripción test", InstrumentType.Stringed, 500, 10, 1).Result;
            var windInstrument = Instrument.Create("Flauta", "Descripción test", InstrumentType.Wind, 300, 5, 1).Result;
            var keyboardInstrument = Instrument.Create("Piano", "Descripción test", InstrumentType.keyboard, 1000, 2, 1).Result;

            await _adapter.CreateAsync(stringedInstrument);
            await _adapter.CreateAsync(windInstrument);
            await _adapter.CreateAsync(keyboardInstrument);

            int expectedChanges = 3;
            int saveResult = await SaveChangesAsync();

            // Act
            var stringedResult = await _adapter.GetStockByType(InstrumentType.Stringed);
            var windResult = await _adapter.GetStockByType(InstrumentType.Wind);
            var keyboardResult = await _adapter.GetStockByType(InstrumentType.keyboard);

            // Assert
            Assert.Equal(10, stringedResult.Result);
            Assert.Equal(5, windResult.Result);
            Assert.Equal(2, keyboardResult.Result);
            Assert.Equal(expectedChanges, saveResult);
        }

        [Fact]
        public void Adapter_ShouldImplementIInstrumentsRepositoryPort()
        {
            // Assert
            Assert.IsAssignableFrom<IInstrumentsRepositoryPort>(_adapter);
        }

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnInstrumentsOrderedByCreationDateAsc()
        //{
        //    // Arrange
        //    var instrument1 = Instrument.Create("Guitarra Primera", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
        //    var instrument2 = Instrument.Create("Guitarra Segunda", "Descripción test", InstrumentType.Stringed, 500, 15).Result;
        //    var instrument3 = Instrument.Create("Guitarra Tercera", "Descripción test", InstrumentType.Stringed, 500, 20).Result;

        //    await _adapter.CreateAsync(instrument1);
        //    await Task.Delay(10);
        //    await _adapter.CreateAsync(instrument2);
        //    await Task.Delay(10);
        //    await _adapter.CreateAsync(instrument3);

        //    int expectedChanges = 3;
        //    int saveResult = await SaveChangesAsync();

        //    // Act
        //    var result = await _adapter.GetAllAsync(SortDirection.Asc);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.HasErrors);
        //    Assert.NotNull(result.Result);
        //    Assert.Equal(3, result.Result.Count);
        //    Assert.Equal("Guitarra Primera", result.Result[0].Name);
        //    Assert.Equal("Guitarra Segunda", result.Result[1].Name);
        //    Assert.Equal("Guitarra Tercera", result.Result[2].Name);
        //    Assert.Equal(expectedChanges, saveResult);
        //}

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnInstrumentsOrderedByCreationDateDesc()
        //{
        //    // Arrange
        //    var instrument1 = Instrument.Create("Guitarra Primera", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
        //    var instrument2 = Instrument.Create("Guitarra Segunda", "Descripción test", InstrumentType.Stringed, 500, 15).Result;
        //    var instrument3 = Instrument.Create("Guitarra Tercera", "Descripción test", InstrumentType.Stringed, 500, 20).Result;

        //    await _adapter.CreateAsync(instrument1);
        //    await Task.Delay(10);
        //    await _adapter.CreateAsync(instrument2);
        //    await Task.Delay(10);
        //    await _adapter.CreateAsync(instrument3);

        //    int expectedChanges = 3;
        //    int saveResult = await SaveChangesAsync();

        //    // Act
        //    var result = await _adapter.GetAllAsync(SortDirection.Desc);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.HasErrors);
        //    Assert.NotNull(result.Result);
        //    Assert.Equal(3, result.Result.Count);
        //    Assert.Equal("Guitarra Tercera", result.Result[0].Name);
        //    Assert.Equal("Guitarra Segunda", result.Result[1].Name);
        //    Assert.Equal("Guitarra Primera", result.Result[2].Name);
        //    Assert.Equal(expectedChanges, saveResult);
        //}

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

        //    int expectedChanges = 3;
        //    await SaveChangesAsync();

        //    var idsToDelete = new List<string> { instrument1.Id, instrument2.Id, instrument3.Id };

        //    // Act
        //    var result = await _adapter.DeleteMultipleAsync(idsToDelete);
        //    var deleteSaveResult = await SaveChangesAsync();
        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.HasErrors);
        //    Assert.Equal(expectedChanges, result.Result);
        //    Assert.Equal(expectedChanges, deleteSaveResult);

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

        //    int expectedChanges = 2;
        //    int saveResult = await SaveChangesAsync();

        //    var idsToDelete = new List<string> { instrument1.Id, "non-existent-id", instrument2.Id };

        //    // Act
        //    var result = await _adapter.DeleteMultipleAsync(idsToDelete);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.True(result.HasErrors);
        //    Assert.Equal(ErrorCode.NOT_FOUND, result.Errors[0].ErrorCode);
        //    Assert.Contains("non-existent-id", result.Errors[0].Message);
        //    Assert.Equal(expectedChanges, saveResult);

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
        //    await SaveChangesAsync();
        //    int expectedChanges = 1;

        //    var idsToDelete = new List<string> { instrument.Id };

        //    // Act
        //    var result = await _adapter.DeleteMultipleAsync(idsToDelete);
        //    int deleteSaveResult = await SaveChangesAsync();
        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.HasErrors);
        //    Assert.Equal(1, result.Result);
        //    Assert.Equal(expectedChanges, deleteSaveResult);

        //    // Verify deletion
        //    var remaining = await _adapter.GetAllAsync();
        //    Assert.Empty(remaining.Result);
        //}

        [Fact]
        public async Task UpdateAsync_ShouldUpdateInstrument_WhenSuccessful()
        {
            // Arrange
            var instrument = Instrument.Create(
                "Guitarra Original",
                "Descripción original",
                InstrumentType.Stringed,
                500,
                10,
                1
            ).Result;
            await _adapter.CreateAsync(instrument);
            await SaveChangesAsync();
            int expectedChanges = 1;

            var resultUpdated = instrument.Update("Guitarra Actualizada", "Descripción actualizada", InstrumentType.Stringed);

            // Act
            _adapter.Update(instrument);
            int saveResult = await SaveChangesAsync();

            // Assert
            Assert.Equal(expectedChanges, saveResult);
            Assert.NotNull(resultUpdated.Result);
            Assert.Equal(instrument.Name, resultUpdated.Result.Name);
            Assert.Equal(instrument.Description, resultUpdated.Result.Description);
            Assert.Equal(instrument.Type, resultUpdated.Result.Type);
            Assert.True(resultUpdated.IsSuccess);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
        private async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }
    }
}
