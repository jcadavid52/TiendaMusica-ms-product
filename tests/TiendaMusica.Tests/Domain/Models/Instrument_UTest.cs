using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;

namespace TiendaMusica.Tests.Domain.Models
{
    public class Instrument_UTest
    {
        [Fact]
        public void Create_ShouldReturnInstrument_WhenValidParameters()
        {
            var result = Instrument.Create(
                "Guitarra",
                "Descripcion válida de más de diez",
                InstrumentType.Stringed,
                500m,
                10,
                1);

            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
            Assert.Equal("Guitarra", result.Result.Name);
        }

        [Fact]
        public void Create_ShouldReturnValidationError_WhenPriceBelowShippingCost()
        {
            var result = Instrument.Create("Guitarra", "Descripcion válida", InstrumentType.Stringed, 50m, 10,1);

            Assert.NotNull(result);
            Assert.True(result.HasErrors);
            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
        }

        [Fact]
        public void Create_ShouldReturnValidationError_WhenBundleWithShortDescription()
        {
            var result = Instrument.Create("Guitar Pack", "short", InstrumentType.Stringed, 500m, 5,1);

            Assert.NotNull(result);
            Assert.True(result.HasErrors);
            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
        }

        [Fact]
        public void Create_ShouldSucceed_WhenBundleWithLongDescription()
        {
            var result = Instrument.Create("Guitar Pack", "Descripción larga válida para un bundle", InstrumentType.Stringed, 500m, 5,1);

            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public void Create_ShouldThrow_WhenStockNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Instrument.Create("Guitarra", "Descripcion válida", InstrumentType.Stringed, 500m, -1,1));
        }
    }
}
