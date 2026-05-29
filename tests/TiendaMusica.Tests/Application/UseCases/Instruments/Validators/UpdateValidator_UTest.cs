using Microsoft.Extensions.Logging;
using Moq;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments.Validators;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Domain.Services;

namespace TiendaMusica.Tests.Application.UseCases.Instruments.Validators
{
    public class UpdateValidator_UTest
    {
        private readonly Mock<IInstrumentsRepositoryPort> _instrumentsRepositoryPortMock;
        private readonly Mock<IInstrumentValidationService> _instrumentValidationServiceMock;
        private readonly Mock<ILogger<UpdateValidator>> _loggerMock;
        private readonly UpdateValidator _validator;

        public UpdateValidator_UTest()
        {
            _instrumentsRepositoryPortMock = new Mock<IInstrumentsRepositoryPort>();
            _instrumentValidationServiceMock = new Mock<IInstrumentValidationService>();
            _loggerMock = new Mock<ILogger<UpdateValidator>>();

            _validator = new UpdateValidator(
                _instrumentsRepositoryPortMock.Object,
                _instrumentValidationServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task ValidateAsync_ValidCommand_ReturnsSuccess()
        {
            var command = new InstrumentUpdateCommand("id1", "newName", "desc", InstrumentType.Stringed);
            var existingInstrument = Instrument.Create("oldName", "desc", InstrumentType.Stringed, 150, 10).Result!;
            existingInstrument.GetType().GetProperty("Id")!.SetValue(existingInstrument, "id1");

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("id1"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = existingInstrument
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync("newName"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = null
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(InstrumentType.Stringed))
                .ReturnsAsync(new Results<int>
                {
                    Result = 5
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateLimitStockByType(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstrumentType>()))
                .Returns(new Results<bool>
                {
                    Result = true
                });

            var result = await _validator.ValidateAsync(command);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_ValidCommandWithTypeChange_ReturnsSuccess()
        {
            var command = new InstrumentUpdateCommand("id1", "newName", "desc", InstrumentType.Wind);
            var existingInstrument = Instrument.Create("oldName", "desc", InstrumentType.Stringed, 150, 10).Result!;
            existingInstrument.GetType().GetProperty("Id")!.SetValue(existingInstrument, "id1");

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("id1"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = existingInstrument
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync("newName"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = null
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(InstrumentType.Wind))
                .ReturnsAsync(new Results<int>
                {
                    Result = 5
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateLimitStockByType(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstrumentType>()))
                .Returns(new Results<bool>
                {
                    Result = true
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(InstrumentType.Stringed))
                .ReturnsAsync(new Results<int>
                {
                    Result = 3
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateMinimumStockAfterUpdate(
                It.IsAny<int>(), It.IsAny<InstrumentType>()))
                .Returns(new Results<bool>
                {
                    Result = true
                });

            var result = await _validator.ValidateAsync(command);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Result);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WhenGetByIdReturnsFailure_ReturnsServerErrorCode()
        {
            var command = new InstrumentUpdateCommand("id1", "newName", "desc", InstrumentType.Stringed);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("id1"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error al obtener instrumento")
                    }
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.SERVER_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenInstrumentNotFound_ReturnsNotFound()
        {
            var command = new InstrumentUpdateCommand("id1", "newName", "desc", InstrumentType.Stringed);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("id1"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = null
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.NOT_FOUND, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenGetByNameReturnsFailure_ReturnsServerErrorCode()
        {
            var command = new InstrumentUpdateCommand("id1", "newName", "desc", InstrumentType.Stringed);
            var existingInstrument = Instrument.Create("oldName", "desc", InstrumentType.Stringed, 150, 10).Result!;
            existingInstrument.GetType().GetProperty("Id")!.SetValue(existingInstrument, "id1");

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("id1"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = existingInstrument
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync("newName"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error al buscar nombre")
                    }
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.SERVER_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenNameConflict_ReturnsConflictError()
        {
            var command = new InstrumentUpdateCommand("id1", "existingName", "desc", InstrumentType.Stringed);
            var existingInstrument = Instrument.Create("oldName", "desc", InstrumentType.Stringed, 150, 10).Result!;
            existingInstrument.GetType().GetProperty("Id")!.SetValue(existingInstrument, "id1");

            var conflictingInstrument = Instrument.Create("existingName", "desc", InstrumentType.Stringed, 150, 20).Result!;
            conflictingInstrument.GetType().GetProperty("Id")!.SetValue(conflictingInstrument, "id2");

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("id1"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = existingInstrument
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync("existingName"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = conflictingInstrument
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.CONFLICT_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenGetStockByTypeReturnsFailure_ReturnsServerErrorCode()
        {
            var command = new InstrumentUpdateCommand("id1", "newName", "desc", InstrumentType.Stringed);
            var existingInstrument = Instrument.Create("oldName", "desc", InstrumentType.Stringed, 150, 10).Result!;
            existingInstrument.GetType().GetProperty("Id")!.SetValue(existingInstrument, "id1");

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("id1"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = existingInstrument
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync("newName"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = null
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(InstrumentType.Stringed))
                .ReturnsAsync(new Results<int>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error al obtener stock por tipo")
                    }
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.SERVER_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenValidateLimitStockByTypeReturnsFailure_ReturnsErrors()
        {
            var command = new InstrumentUpdateCommand("id1", "newName", "desc", InstrumentType.Stringed);
            var existingInstrument = Instrument.Create("oldName", "desc", InstrumentType.Stringed, 150, 10).Result!;
            existingInstrument.GetType().GetProperty("Id")!.SetValue(existingInstrument, "id1");

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("id1"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = existingInstrument
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync("newName"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = null
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(InstrumentType.Stringed))
                .ReturnsAsync(new Results<int>
                {
                    Result = 5
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateLimitStockByType(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstrumentType>()))
                .Returns(new Results<bool>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.LIMIT_STOCK_ERROR, "Límite de stock excedido")
                    }
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.LIMIT_STOCK_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenTypeChangeAndGetStockByTypeOldFails_ReturnsServerErrorCode()
        {
            var command = new InstrumentUpdateCommand("id1", "newName", "desc", InstrumentType.Wind);
            var existingInstrument = Instrument.Create("oldName", "desc", InstrumentType.Stringed, 150, 10).Result!;
            existingInstrument.GetType().GetProperty("Id")!.SetValue(existingInstrument, "id1");

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("id1"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = existingInstrument
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync("newName"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = null
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(InstrumentType.Wind))
                .ReturnsAsync(new Results<int>
                {
                    Result = 5
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateLimitStockByType(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstrumentType>()))
                .Returns(new Results<bool>
                {
                    Result = true
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(InstrumentType.Stringed))
                .ReturnsAsync(new Results<int>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error al obtener stock del tipo actual")
                    }
                });

            var result = await _validator.ValidateAsync(command);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.SERVER_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenTypeChangeAndValidateMinimumStockAfterUpdateFails_ReturnsErrors()
        {
            var command = new InstrumentUpdateCommand("id1", "newName", "desc", InstrumentType.Wind);
            var existingInstrument = Instrument.Create("oldName", "desc", InstrumentType.Stringed, 150, 10).Result!;
            existingInstrument.GetType().GetProperty("Id")!.SetValue(existingInstrument, "id1");

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("id1"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = existingInstrument
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync("newName"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = null
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(InstrumentType.Wind))
                .ReturnsAsync(new Results<int>
                {
                    Result = 5
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateLimitStockByType(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstrumentType>()))
                .Returns(new Results<bool>
                {
                    Result = true
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(InstrumentType.Stringed))
                .ReturnsAsync(new Results<int>
                {
                    Result = 1
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateMinimumStockAfterUpdate(
                It.IsAny<int>(), It.IsAny<InstrumentType>()))
                .Returns(new Results<bool>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.MINIMUM_STOCK_ERROR, "Stock mínimo no alcanzado")
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
