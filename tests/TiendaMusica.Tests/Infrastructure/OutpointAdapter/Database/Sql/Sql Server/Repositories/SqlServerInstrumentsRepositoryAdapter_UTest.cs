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

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoDataInDatabase()
        {
            // Act
            var result = await _adapter.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Empty(result.Result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstruments_WhenRepositoryHasData()
        {
            // Arrange
            var instrument1 = Instrument.Create("Guitarra Eléctrica", "Descripción test 1", InstrumentType.Stringed, 500, 10).Result;
            var instrument2 = Instrument.Create("Guitarra Acústica", "Descripción test 2", InstrumentType.Stringed, 500, 15).Result;

            await _adapter.CreateAsync(instrument1);
            await _adapter.CreateAsync(instrument2);

            // Act
            var result = await _adapter.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Equal(2, result.Result.Count);
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
        public void Adapter_ShouldImplementIInstrumentsRepositoryPort()
        {
            // Assert
            Assert.IsAssignableFrom<IInstrumentsRepositoryPort>(_adapter);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsOrderedByCreationDateAsc()
        {
            // Arrange
            var instrument1 = Instrument.Create("Guitarra Primera", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            var instrument2 = Instrument.Create("Guitarra Segunda", "Descripción test", InstrumentType.Stringed, 500, 15).Result;
            var instrument3 = Instrument.Create("Guitarra Tercera", "Descripción test", InstrumentType.Stringed, 500, 20).Result;

            await _adapter.CreateAsync(instrument1);
            await Task.Delay(10);
            await _adapter.CreateAsync(instrument2);
            await Task.Delay(10);
            await _adapter.CreateAsync(instrument3);

            // Act
            var result = await _adapter.GetAllAsync(SortDirection.Asc);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Equal(3, result.Result.Count);
            Assert.Equal("Guitarra Primera", result.Result[0].Name);
            Assert.Equal("Guitarra Segunda", result.Result[1].Name);
            Assert.Equal("Guitarra Tercera", result.Result[2].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsOrderedByCreationDateDesc()
        {
            // Arrange
            var instrument1 = Instrument.Create("Guitarra Primera", "Descripción test", InstrumentType.Stringed, 500, 10).Result;
            var instrument2 = Instrument.Create("Guitarra Segunda", "Descripción test", InstrumentType.Stringed, 500, 15).Result;
            var instrument3 = Instrument.Create("Guitarra Tercera", "Descripción test", InstrumentType.Stringed, 500, 20).Result;

            await _adapter.CreateAsync(instrument1);
            await Task.Delay(10);
            await _adapter.CreateAsync(instrument2);
            await Task.Delay(10);
            await _adapter.CreateAsync(instrument3);

            // Act
            var result = await _adapter.GetAllAsync(SortDirection.Desc);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Equal(3, result.Result.Count);
            Assert.Equal("Guitarra Tercera", result.Result[0].Name);
            Assert.Equal("Guitarra Segunda", result.Result[1].Name);
            Assert.Equal("Guitarra Primera", result.Result[2].Name);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
