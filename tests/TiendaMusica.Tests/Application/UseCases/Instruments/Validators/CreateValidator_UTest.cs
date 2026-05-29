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
    public class CreateValidator_UTest
    {
        private readonly Mock<IInstrumentsRepositoryPort> _instrumentsRepositoryPortMock;
        private readonly Mock<IInstrumentValidationService> _instrumentValidationServiceMock;
        private readonly Mock<ILogger<CreateValidator>> _loggerMock;
        private readonly CreateValidator _validator;

        public CreateValidator_UTest()
        {
            _instrumentsRepositoryPortMock = new Mock<IInstrumentsRepositoryPort>();
            _instrumentValidationServiceMock = new Mock<IInstrumentValidationService>();
            _loggerMock = new Mock<ILogger<CreateValidator>>();

            _validator = new CreateValidator(
                _instrumentsRepositoryPortMock.Object,
                _instrumentValidationServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task ValidateAsync_ValidCommand_ReturnsSuccess()
        {
            // Arrange
            var command = new InstrumentCreateCommand("nombre test", "descripcion test", InstrumentType.Stringed, 150, 1);
            int countStockByType = 1;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
                .ReturnsAsync(new Results<int>
                {
                    Result = countStockByType
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateLimitStockByType(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<InstrumentType>()
                ))
                .Returns(new Results<bool>
                {
                    Result = true
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = null
                });

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Result);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WhenInstrumentExists_ReturnsFailure()
        {
            // Arrange
            var command = new InstrumentCreateCommand("nombre test", "descripcion test", InstrumentType.Stringed, 150, 1);
            int countStockByType = 1;
            var instrument = Instrument.Create("nombre test", "descripcion test", InstrumentType.Stringed, 150, 1);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
                .ReturnsAsync(new Results<int>
                {
                    Result = countStockByType
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateLimitStockByType(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<InstrumentType>()
                ))
                .Returns(new Results<bool>
                {
                    Result = true
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = instrument.Result
                });

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.CONFLICT_ERROR, result.Errors.First().ErrorCode);
        }


        [Fact]
        public async Task ValidateAsync_WhenStockByTypeReturnsFailure_ReturnsServerErrorCode()
        {
            // Arrange
            var command = new InstrumentCreateCommand("nombre test", "descripcion test", InstrumentType.Stringed, 150, 1);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
                .ReturnsAsync(new Results<int>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error al obtener el stock por tipo")
                        
                    }
                });


            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.SERVER_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenValidateLimitStockByTypeReturnsFailure_ReturnsServerErrorCode()
        {
            // Arrange
            var command = new InstrumentCreateCommand("nombre test", "descripcion test", InstrumentType.Stringed, 150, 1);
            int countStockByType = 1;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
                .ReturnsAsync(new Results<int>
                {
                    Result = countStockByType
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateLimitStockByType(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<InstrumentType>()
                ))
                .Returns(new Results<bool>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error al validar el límite de stock por tipo")

                    }
                });

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.SERVER_ERROR, result.Errors.First().ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_WhenGetByNameAsyncReturnsFailure_ReturnsServerErrorCode()
        {
            // Arrange
            var command = new InstrumentCreateCommand("nombre test", "descripcion test", InstrumentType.Stringed, 150, 1);
            int countStockByType = 1;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
                .ReturnsAsync(new Results<int>
                {
                    Result = countStockByType
                });

            _instrumentValidationServiceMock.Setup(service => service.ValidateLimitStockByType(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<InstrumentType>()
                ))
                .Returns(new Results<bool>
                {
                    Result = true
                });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error al obtener el instrumento por nombre")
                    }
                });

            // Act
            var result = await _validator.ValidateAsync(command);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.Result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(ErrorCode.SERVER_ERROR, result.Errors.First().ErrorCode);
        }
    }
}
