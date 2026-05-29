using Microsoft.Extensions.Logging;
using Moq;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments.Validators;
using TiendaMusica.Domain.Dtos;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Domain.Services;

namespace TiendaMusica.Tests.Application.UseCases.Instruments.Validators
{
    public class DeleteMassiveValidator_UTest
    {
        private readonly Mock<IInstrumentsRepositoryPort> _instrumentsRepositoryPortMock;
        private readonly Mock<IInstrumentValidationService> _instrumentValidationServiceMock;
        private readonly Mock<ILogger<DeleteMassiveValidator>> _loggerMock;
        private readonly DeleteMassiveValidator _validator;

        public DeleteMassiveValidator_UTest()
        {
            _instrumentsRepositoryPortMock = new Mock<IInstrumentsRepositoryPort>();
            _instrumentValidationServiceMock = new Mock<IInstrumentValidationService>();
            _loggerMock = new Mock<ILogger<DeleteMassiveValidator>>();

            _validator = new DeleteMassiveValidator(
                _instrumentsRepositoryPortMock.Object,
                _instrumentValidationServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task ValidateAsync_ValidCommand_ReturnsSuccess()
        {
            var instrumentIds = new List<string> { "id1", "id2" };
            var command = new InstrumentDeleteMultipleCommand(instrumentIds);
            var instruments = new List<Instrument>
            {
                Instrument.Create("test1", "desc1", InstrumentType.Stringed, 150, 10).Result!,
                Instrument.Create("test2", "desc2", InstrumentType.Stringed, 150, 20).Result!,
            };
            var stockSummaries = new List<InstrumentStockSummary>
            {
                new InstrumentStockSummary(InstrumentType.Stringed, 30)
            };

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdsAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = instruments
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockSummaryByInstrumentTypesAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync(new Results<IList<InstrumentStockSummary>>
                {
                    Result = stockSummaries
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateMinimumStockAfterDeletion(
                It.IsAny<IList<InstrumentStockSummary>>(),
                It.IsAny<IList<Instrument>>()
            ))
                .Returns(new Results<bool>
                {
                    Result = true
                });

            var result = await _validator.ValidateAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(instruments, result.Result);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WhenInstrumentIdsIsNull_ReturnsValidationError()
        {
            var command = new InstrumentDeleteMultipleCommand(null!);

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.VALIDATION_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenInstrumentIdsIsEmpty_ReturnsValidationError()
        {
            var command = new InstrumentDeleteMultipleCommand(new List<string>());

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.VALIDATION_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenInstrumentIdsContainsInvalidIds_ReturnsValidationError()
        {
            var command = new InstrumentDeleteMultipleCommand(new List<string> { "id1", "", "  " });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.VALIDATION_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenGetByIdsAsyncReturnsFailure_ReturnsServerErrorCode()
        {
            var instrumentIds = new List<string> { "id1", "id2" };
            var command = new InstrumentDeleteMultipleCommand(instrumentIds);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdsAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error al obtener instrumentos por IDs")
                    }
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.SERVER_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenInstrumentCountMismatch_ReturnsNotFound()
        {
            var instrumentIds = new List<string> { "id1", "id2", "id3" };
            var command = new InstrumentDeleteMultipleCommand(instrumentIds);
            var instruments = new List<Instrument>
            {
                Instrument.Create("test1", "desc1", InstrumentType.Stringed, 150, 10).Result!,
                Instrument.Create("test2", "desc2", InstrumentType.Stringed, 150, 20).Result!,
            };

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdsAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = instruments
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.NOT_FOUND, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenGetStockSummaryReturnsFailure_ReturnsServerErrorCode()
        {
            var instrumentIds = new List<string> { "id1", "id2" };
            var command = new InstrumentDeleteMultipleCommand(instrumentIds);
            var instruments = new List<Instrument>
            {
                Instrument.Create("test1", "desc1", InstrumentType.Stringed, 150, 10).Result!,
                Instrument.Create("test2", "desc2", InstrumentType.Stringed, 150, 20).Result!,
            };

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdsAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = instruments
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockSummaryByInstrumentTypesAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync(new Results<IList<InstrumentStockSummary>>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error al obtener resumen de stock")
                    }
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.SERVER_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenValidateMinimumStockAfterDeletionReturnsFailure_ReturnsErrors()
        {
            var instrumentIds = new List<string> { "id1", "id2" };
            var command = new InstrumentDeleteMultipleCommand(instrumentIds);
            var instruments = new List<Instrument>
            {
                Instrument.Create("test1", "desc1", InstrumentType.Stringed, 150, 10).Result!,
                Instrument.Create("test2", "desc2", InstrumentType.Stringed, 150, 20).Result!,
            };
            var stockSummaries = new List<InstrumentStockSummary>
            {
                new InstrumentStockSummary(InstrumentType.Stringed, 30)
            };

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdsAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = instruments
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockSummaryByInstrumentTypesAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync(new Results<IList<InstrumentStockSummary>>
                {
                    Result = stockSummaries
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateMinimumStockAfterDeletion(
                It.IsAny<IList<InstrumentStockSummary>>(),
                It.IsAny<IList<Instrument>>()
            ))
                .Returns(new Results<bool>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.MINIMUM_STOCK_ERROR, "No se puede eliminar, stock mínimo no alcanzado")
                    }
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.MINIMUM_STOCK_ERROR, result.Errors.First().ErrorCode);
        }
    }
}
