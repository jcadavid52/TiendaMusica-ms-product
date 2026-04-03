using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Events;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;
using TiendaMusica.Domain.Services;

namespace TiendaMusica.Tests.Application.UseCases.Instruments
{
    public class InstrumentUseCase_UTest
    {
        private readonly Mock<IInstrumentsRepositoryPort> _instrumentsRepositoryPortMock;
        private readonly Mock<IInstrumentCreateValidationService> _instrumentCreateValidationService;
        private readonly Mock<ILogger<InstrumentUseCase>> _loggerMock;
        private readonly Mock<IMessagePublisherPort> _messagePublisherMock;

        public InstrumentUseCase_UTest()
        {
            _instrumentsRepositoryPortMock = new Mock<IInstrumentsRepositoryPort>();
            _instrumentCreateValidationService = new Mock<IInstrumentCreateValidationService>();
            _loggerMock = new Mock<ILogger<InstrumentUseCase>>();
            _messagePublisherMock = new Mock<IMessagePublisherPort>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstruments_WhenRepositoryReturnsData()
        {
            // Arrange
            var expectedInstruments = new List<Instrument>
            {
                Instrument.Create("Guitarra eléctrica", "Guitarra eléctrica description test", InstrumentType.Stringed,500,10).Result,
                Instrument.Create("Guitarra acústica", "Guitarra acústica description test", InstrumentType.Stringed,500,10).Result
            };

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetAllAsync(
                It.IsAny<SortDirection>(),
                It.IsAny<Expression<Func<Instrument, bool>>[]>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = expectedInstruments
                });
            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);
            // Act
            var result = await useCase.GetAllAsync();
            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.Equal(expectedInstruments.Count, result.Result.Count);
            Assert.Equal(expectedInstruments[0].Name, result.Result[0].Name);
            Assert.Equal(expectedInstruments[1].Name, result.Result[1].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsSortDirectionAsc_WhenUseCaseReturnsData()
        {
            // Arrange
            var query = new GetAllInstrumentQuery(SortDirection.Asc, null, 10, 1);
            var expectedInstruments = new List<Instrument>
            {
                Instrument.Create("Guitarra eléctrica", "Guitarra eléctrica description test", InstrumentType.Stringed,500,10).Result,
                Instrument.Create("Piano", "Piano description test", InstrumentType.keyboard,1000,5).Result,
                Instrument.Create("Saxofón", "Saxofón description test", InstrumentType.Wind,800,8).Result
            };

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetAllAsync(
                SortDirection.Asc,
                It.IsAny<Expression<Func<Instrument, bool>>[]>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = expectedInstruments
                });

            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);

            // Act
            var result = await useCase.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedInstruments.Count, result.Result.Count);
            Assert.Equal("Guitarra eléctrica", result.Result[0].Name);
            Assert.Equal("Piano", result.Result[1].Name);
            Assert.Equal("Saxofón", result.Result[2].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsSortDirectionDesc_WhenUseCaseReturnsData()
        {
            // Arrange
            var query = new GetAllInstrumentQuery(SortDirection.Desc, null, 10, 1);
            var expectedInstruments = new List<Instrument>
            {
                Instrument.Create("Saxofón", "Saxofón description test", InstrumentType.Wind,800,8).Result,
                Instrument.Create("Piano", "Piano description test", InstrumentType.keyboard,1000,5).Result,
                Instrument.Create("Guitarra eléctrica", "Guitarra eléctrica description test", InstrumentType.Stringed,500,10).Result
            };

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetAllAsync(
                SortDirection.Desc,
                It.IsAny<Expression<Func<Instrument, bool>>[]>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = expectedInstruments
                });

            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);

            // Act
            var result = await useCase.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedInstruments.Count, result.Result.Count);
            Assert.Equal("Saxofón", result.Result[0].Name);
            Assert.Equal("Piano", result.Result[1].Name);
            Assert.Equal("Guitarra eléctrica", result.Result[2].Name);
        }

        [Fact]
        public async Task GetAllAsync_WhenRepositoryReturnsErrors_ReturnsFailureResult()
        {
            // Arrange
            _instrumentsRepositoryPortMock.Setup(repo => repo.GetAllAsync(
                It.IsAny<SortDirection>(),
                It.IsAny<Expression<Func<Instrument, bool>>[]>(),
                It.IsAny<int?>(),
                It.IsAny<int?>())
                )
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                    }
                });
            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);
            // Act
            var result = await useCase.GetAllAsync();
            // Assert
            Assert.Null(result.Result);
            Assert.True(result.HasErrors);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateInstrument_WhenValidationPasses()
        {
            // Arrange
            var createCommand = new CreateInstrumentCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = 10 });

            _instrumentCreateValidationService.Setup(service => service.ValidateLimitStockByType(createCommand.Stock, 10, createCommand.Type))
                .Returns(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(createCommand.Name))
                .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Results<Instrument> { Result = Instrument.Create(createCommand.Name, createCommand.Description, createCommand.Type, createCommand.Price, createCommand.Stock).Result });

            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);
            // Act
            var result = await useCase.CreateAsync(createCommand);
            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnsFailureResult_WhenGetStockByTypeReturnsErrors()
        {
            // Arrange
            var createCommand = new CreateInstrumentCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                         new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                    }
                });

            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);
            // Act
            var result = await useCase.CreateAsync(createCommand);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnsFailureResult_WhenValidateLimitStockReturnsErrors()
        {
            // Arrange
            var createCommand = new CreateInstrumentCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            int currenStock = 10;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = currenStock });

            _instrumentCreateValidationService.Setup(service => service.ValidateLimitStockByType(createCommand.Stock, currenStock, createCommand.Type))
                .Returns(new Results<bool>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                         new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                    }
                });

            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);
            // Act
            var result = await useCase.CreateAsync(createCommand);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnsFailureResult_WhenExistingInstrument()
        {
            // Arrange
            var createCommand = new CreateInstrumentCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            int currentStock = 10;
            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentCreateValidationService.Setup(service => service.ValidateLimitStockByType(createCommand.Stock, currentStock, createCommand.Type))
                .Returns(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(createCommand.Name))
                .ReturnsAsync(new Results<Instrument?> { Result = Instrument.Create(createCommand.Name, createCommand.Description, createCommand.Type, createCommand.Price, createCommand.Stock).Result });

            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);
            // Act
            var result = await useCase.CreateAsync(createCommand);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnsFailureResult_WhenGetByNameAsyncReturnsErrors()
        {
            // Arrange
            var createCommand = new CreateInstrumentCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            int currentSock = 0;
            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = currentSock });

            _instrumentCreateValidationService.Setup(service => service.ValidateLimitStockByType(createCommand.Stock, currentSock, createCommand.Type))
                .Returns(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(createCommand.Name))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                         new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                    }
                });

            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);
            // Act
            var result = await useCase.CreateAsync(createCommand);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnsFailureResult_WhenCreateAsyncReturnsErrors()
        {
            // Arrange
            var createCommand = new CreateInstrumentCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            int currentStock = 10;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentCreateValidationService.Setup(service => service.ValidateLimitStockByType(createCommand.Stock, currentStock, createCommand.Type))
                .Returns(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(createCommand.Name))
                .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                         new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                    }
                });

            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);
            // Act
            var result = await useCase.CreateAsync(createCommand);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnsFailureResult_WhenCreateAsyncThrowArgumentException()
        {
            // Arrange
            var createCommand = new CreateInstrumentCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            int currentStock = 10;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentCreateValidationService.Setup(service => service.ValidateLimitStockByType(createCommand.Stock, currentStock, createCommand.Type))
                .Returns(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(createCommand.Name))
                .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .Throws(new ArgumentException("Exception forzada en el UseCase"));

            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);
            // Act
            var result = await useCase.CreateAsync(createCommand);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateInstrument_WhenPublishEventSuccess()
        {
            int currentStock = 10;
            var createCommand = new CreateInstrumentCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
                .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentCreateValidationService.Setup(service => service.ValidateLimitStockByType(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstrumentType>()))
                .Returns(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
               .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = Instrument.Create(createCommand.Name, createCommand.Description, createCommand.Type, createCommand.Price, createCommand.Stock).Result
                });

            _messagePublisherMock.Setup(publisher => publisher.PublishAsync(It.IsAny<InstrumentCreatedEvent>())).Returns(Task.CompletedTask);


            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);

            var result = await useCase.CreateAsync(createCommand);

             Assert.NotNull(result);
             Assert.True(result.IsSuccess);
             Assert.False(result.HasErrors);
            _messagePublisherMock.Verify(
                publisher => publisher.PublishAsync(It.IsAny<InstrumentCreatedEvent>()),
                Times.Once());
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowsException_WhenPublishEventFailure()
        {
            int currentStock = 10;
            var createCommand = new CreateInstrumentCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
                .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentCreateValidationService.Setup(service => service.ValidateLimitStockByType(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstrumentType>()))
                .Returns(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
               .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = Instrument.Create(createCommand.Name, createCommand.Description, createCommand.Type, createCommand.Price, createCommand.Stock).Result
                });

            _messagePublisherMock.Setup(publisher => publisher.PublishAsync(It.IsAny<InstrumentCreatedEvent>()))
                .ThrowsAsync(new Exception("Error al publicar el evento de instrumento creado"));

            var useCase = new InstrumentUseCase(_instrumentsRepositoryPortMock.Object, _instrumentCreateValidationService.Object, _loggerMock.Object, _messagePublisherMock.Object);

            await Assert.ThrowsAsync<Exception>(() => useCase.CreateAsync(createCommand));
        }
    }
}
