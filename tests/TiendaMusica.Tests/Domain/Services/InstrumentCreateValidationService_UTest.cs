using Moq;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Services;

namespace TiendaMusica.Tests.Domain.Services
{
    public class InstrumentCreateValidationService_UTest
    {
        private readonly InstrumentCreateValidationService _service = new();

        [Fact]
        public void ValidateLimitStockByType_ShouldReturnSuccess_WhenTotalLessThanLimit_Stringed()
        {
            int limit = (int)InstrumentLimitStockByType.Stringed;

            var result = _service.ValidateLimitStockByType(limit - 1, 0, InstrumentType.Stringed);

            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
        }

        [Fact]
        public void ValidateLimitStockByType_ShouldReturnError_WhenTotalEqualsOrExceedsLimit_Stringed()
        {
            int limit = (int)InstrumentLimitStockByType.Stringed;

            var result = _service.ValidateLimitStockByType(limit, 0, InstrumentType.Stringed);

            Assert.NotNull(result);
            Assert.True(result.HasErrors);
            Assert.False(result.IsSuccess);
            Assert.False(result.Result);
        }

        [Fact]
        public void ValidateLimitStockByType_ShouldReturnSuccess_WhenTotalLessThanLimit_Wind()
        {
            int limit = (int)InstrumentLimitStockByType.Wind;

            var result = _service.ValidateLimitStockByType(limit - 1, 0, InstrumentType.Wind);

            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
        }

        [Fact]
        public void ValidateLimitStockByType_ShouldReturnError_WhenTotalEqualsOrExceedsLimit_Keyboard()
        {
            int limit = (int)InstrumentLimitStockByType.keyboard;

            var result = _service.ValidateLimitStockByType(limit, 0, InstrumentType.keyboard);

            Assert.NotNull(result);
            Assert.True(result.HasErrors);
            Assert.False(result.IsSuccess);
            Assert.False(result.Result);
        }
    }
}
