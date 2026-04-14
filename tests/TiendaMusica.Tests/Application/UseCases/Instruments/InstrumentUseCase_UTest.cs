using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Application.Validators.Instruments;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Tests.Application.UseCases.Instruments
{
    public class InstrumentUseCase_UTest
    {
        private readonly Mock<IInstrumentsRepositoryPort> _instrumentsRepositoryPortMock;
        private readonly Mock<ILogger<InstrumentUseCase>> _loggerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<DomainEventsCollector> _domainEventsCollectorMock;
        private readonly Mock<ICachePort> _cachePortMock;
        private readonly Mock<IInstrumentValidator<InstrumentUpdateCommand, Instrument>> _updateValidatorMock;
        private readonly Mock<IInstrumentValidator<InstrumentCreateCommand, bool>> _createValidatorMock;
        private readonly Mock<IInstrumentValidator<InstrumentDeleteMultipleCommand, IList<Instrument>>> _deleteMassiveValidatorMock;

        public InstrumentUseCase_UTest()
        {
            _instrumentsRepositoryPortMock = new Mock<IInstrumentsRepositoryPort>();
            _loggerMock = new Mock<ILogger<InstrumentUseCase>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _domainEventsCollectorMock = new Mock<DomainEventsCollector>();
            _cachePortMock = new Mock<ICachePort>();
            _updateValidatorMock = new Mock<IInstrumentValidator<InstrumentUpdateCommand, Instrument>>();
            _createValidatorMock = new Mock<IInstrumentValidator<InstrumentCreateCommand, bool>>();
            _deleteMassiveValidatorMock = new Mock<IInstrumentValidator<InstrumentDeleteMultipleCommand, IList<Instrument>>>();
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

            var useCase = new InstrumentUseCase(
                _instrumentsRepositoryPortMock.Object,
                _updateValidatorMock.Object,
                _createValidatorMock.Object,
                _deleteMassiveValidatorMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _domainEventsCollectorMock.Object,
                _cachePortMock.Object
                );

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
            var query = new InstrumentGetAllQuery(SortDirection.Asc, null, 10, 1);
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

            var useCase = new InstrumentUseCase(
                _instrumentsRepositoryPortMock.Object,
                _updateValidatorMock.Object,
                _createValidatorMock.Object,
                _deleteMassiveValidatorMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _domainEventsCollectorMock.Object,
                _cachePortMock.Object
                );

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
            var query = new InstrumentGetAllQuery(SortDirection.Desc, null, 10, 1);
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

            var useCase = new InstrumentUseCase(
                _instrumentsRepositoryPortMock.Object,
                _updateValidatorMock.Object,
                _createValidatorMock.Object,
                _deleteMassiveValidatorMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _domainEventsCollectorMock.Object,
                _cachePortMock.Object
                );

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

            var useCase = new InstrumentUseCase(
                _instrumentsRepositoryPortMock.Object,
                _updateValidatorMock.Object,
                _createValidatorMock.Object,
                _deleteMassiveValidatorMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _domainEventsCollectorMock.Object,
                _cachePortMock.Object
                );

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
            var createCommand = new InstrumentCreateCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            bool expectedChanges = true;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = 10 });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(createCommand.Name))
                .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Results<Instrument> { Result = Instrument.Create(createCommand.Name, createCommand.Description, createCommand.Type, createCommand.Price, createCommand.Stock).Result });

            var useCase = new InstrumentUseCase(
                _instrumentsRepositoryPortMock.Object,
                _updateValidatorMock.Object,
                _createValidatorMock.Object,
                _deleteMassiveValidatorMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _domainEventsCollectorMock.Object,
                _cachePortMock.Object
                );

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync<string>(It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Results<bool>
               {
                   Result = expectedChanges
               });

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
            var createCommand = new InstrumentCreateCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                         new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                    }
                });

            var useCase = new InstrumentUseCase(
                _instrumentsRepositoryPortMock.Object,
                _updateValidatorMock.Object,
                _createValidatorMock.Object,
                _deleteMassiveValidatorMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _domainEventsCollectorMock.Object,
                _cachePortMock.Object
                );

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
            var createCommand = new InstrumentCreateCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            int currenStock = 10;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = currenStock });

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

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
            var createCommand = new InstrumentCreateCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            int currentStock = 10;
            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(createCommand.Name))
                .ReturnsAsync(new Results<Instrument?> { Result = Instrument.Create(createCommand.Name, createCommand.Description, createCommand.Type, createCommand.Price, createCommand.Stock).Result });

            var useCase = new InstrumentUseCase(
                _instrumentsRepositoryPortMock.Object,
                _updateValidatorMock.Object,
                _createValidatorMock.Object,
                _deleteMassiveValidatorMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _domainEventsCollectorMock.Object,
                _cachePortMock.Object
                );

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
            var createCommand = new InstrumentCreateCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            int currentSock = 0;
            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = currentSock });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(createCommand.Name))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                         new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                    }
                });

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

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
            var createCommand = new InstrumentCreateCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            int currentStock = 10;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = currentStock });

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

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

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
            var createCommand = new InstrumentCreateCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            int currentStock = 10;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(createCommand.Type))
                .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(createCommand.Name))
                .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .Throws(new ArgumentException("Exception forzada en el UseCase"));

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

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
            //Assert
            int currentStock = 10;
            var createCommand = new InstrumentCreateCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);
            bool expectedChanges = true;

            //Act
            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
                .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
               .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = Instrument.Create(createCommand.Name, createCommand.Description, createCommand.Type, createCommand.Price, createCommand.Stock).Result
                });

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync<string>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Results<bool>
                {
                    Result = expectedChanges
                });

            var useCase = new InstrumentUseCase(
                _instrumentsRepositoryPortMock.Object,
                _updateValidatorMock.Object,
                _createValidatorMock.Object,
                _deleteMassiveValidatorMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _domainEventsCollectorMock.Object,
                _cachePortMock.Object
                );

            var result = await useCase.CreateAsync(createCommand);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowsException_WhenSaveChangesFailure()
        {
            int currentStock = 10;
            var createCommand = new InstrumentCreateCommand("Instrument test", "Instrument test description", InstrumentType.Stringed, 1500, 1);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
                .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
               .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = Instrument.Create(createCommand.Name, createCommand.Description, createCommand.Type, createCommand.Price, createCommand.Stock).Result
                });

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync<string>(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error al guardar los cambios en la base de datos"));

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

            await Assert.ThrowsAsync<Exception>(() => useCase.CreateAsync(createCommand));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnInstrument_WhenInstrumentExists()
        {
            // Arrange
            var expectedInstrument = Instrument.Create("Guitarra eléctrica", "Guitarra eléctrica description test", InstrumentType.Stringed, 500, 10).Result;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync(expectedInstrument.Id))
                .ReturnsAsync(new Results<Instrument?> { Result = expectedInstrument });

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

            // Act
            var result = await useCase.GetByIdAsync(expectedInstrument.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Result);
            Assert.Equal(expectedInstrument.Id, result.Result.Id);
            Assert.Equal(expectedInstrument.Name, result.Result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnError_WhenIdIsEmpty()
        {
            // Arrange
            var id = "";

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
            );

            // Act
            var result = await useCase.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRepositoryReturnsErrors_ReturnsFailureResult()
        {
            // Arrange
            var id = "test-id";

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("test-id"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                    }
                });

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

            // Act
            var result = await useCase.GetByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
            Assert.Null(result.Result);
        }

        //[Fact]
        //public async Task DeleteMultipleAsync_ShouldDeleteInstruments_WhenSuccessful()
        //{
        //    // Arrange
        //    var instrumentIds = new List<string> { "id1", "id2", "id3" };
        //    var command = new InstrumentDeleteMultipleCommand(instrumentIds);
        //    bool expectedChanges = true;
        //    int expectedDeletedCount = instrumentIds.Count;

        //    _instrumentsRepositoryPortMock.Setup(repo => repo.DeleteMultipleAsync(instrumentIds))
        //        .ReturnsAsync(new Results<int> { Result = expectedDeletedCount });

        //    _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync<string>(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new Results<bool>
        //        {
        //            Result = expectedChanges
        //        });

        //    var useCase = new InstrumentUseCase(
        //       _instrumentsRepositoryPortMock.Object,
        //       _instrumentCreateValidationService.Object,
        //       _loggerMock.Object,
        //       _messagePublisherMock.Object,
        //       _unitOfWorkMock.Object
        //       );

        //    // Act
        //    var result = await useCase.DeleteMultipleAsync(command);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.True(result.IsSuccess);
        //    Assert.False(result.HasErrors);
        //}

        [Fact]
        public async Task DeleteMultipleAsync_ShouldReturnError_WhenIdListIsEmpty()
        {
            // Arrange
            var command = new InstrumentDeleteMultipleCommand(new List<string>());

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

            // Act
            var result = await useCase.DeleteMultipleAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
            Assert.Equal(ErrorCode.VALIDATION_ERROR, result.Errors[0].ErrorCode);
        }

        [Fact]
        public async Task DeleteMultipleAsync_ShouldReturnError_WhenIdListContainsEmptyIds()
        {
            // Arrange
            var command = new InstrumentDeleteMultipleCommand(new List<string> { "id1", "", "id3" });

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

            // Act
            var result = await useCase.DeleteMultipleAsync(command);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
            Assert.Equal(ErrorCode.VALIDATION_ERROR, result.Errors[0].ErrorCode);
        }

        //[Fact]
        //public async Task DeleteMultipleAsync_WhenRepositoryReturnsErrors_ReturnsFailureResult()
        //{
        //    // Arrange
        //    var instrumentIds = new List<string> { "id1", "id2" };
        //    var command = new InstrumentDeleteMultipleCommand(instrumentIds);

        //    _instrumentsRepositoryPortMock.Setup(repo => repo.DeleteMultipleAsync(instrumentIds))
        //        .ReturnsAsync(new Results<int>
        //        {
        //            Errors = new List<TiendaMusicaError>
        //            {
        //                new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
        //            }
        //        });

        //    var useCase = new InstrumentUseCase(
        //       _instrumentsRepositoryPortMock.Object,
        //       _instrumentCreateValidationService.Object,
        //       _loggerMock.Object,
        //       _messagePublisherMock.Object,
        //       _unitOfWorkMock.Object
        //       );

        //    // Act
        //    var result = await useCase.DeleteMultipleAsync(command);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.False(result.IsSuccess);
        //    Assert.True(result.HasErrors);
        //}

        [Fact]
        public async Task UpdateAsync_ShouldUpdateInstrument_WhenSuccessful()
        {
            // Arrange
            var existingInstrument = Instrument.Create("Guitarra Original", "Descripción original", InstrumentType.Stringed, 500, 10).Result;
            var updateCommand = new InstrumentUpdateCommand(existingInstrument.Id, "Guitarra Actualizada", "Descripción actualizada", InstrumentType.Stringed);
            bool expectedChanges = true;
            int currentStock = 10;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
              .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(updateCommand.Name))
               .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync(existingInstrument.Id))
                .ReturnsAsync(new Results<Instrument?> { Result = existingInstrument });

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync<string>(It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Results<bool>
               {
                   Result = expectedChanges
               });

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

            // Act
            var result = await useCase.UpdateAsync(updateCommand);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.HasErrors);
            Assert.Equal(existingInstrument.Id, result.Result.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenIdIsEmpty()
        {
            // Arrange
            var updateCommand = new InstrumentUpdateCommand("", "Guitarra Actualizada", "Descripción actualizada", InstrumentType.Stringed);

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

            // Act
            var result = await useCase.UpdateAsync(updateCommand);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
            Assert.Equal(ErrorCode.VALIDATION_ERROR, result.Errors[0].ErrorCode);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenInstrumentNotFound()
        {
            // Arrange
            var updateCommand = new InstrumentUpdateCommand("non-existent-id", "Guitarra Actualizada", "Descripción actualizada", InstrumentType.Stringed);


            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(updateCommand.Name))
               .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("non-existent-id"))
                .ReturnsAsync(new Results<Instrument?> { Result = null });

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

            // Act
            var result = await useCase.UpdateAsync(updateCommand);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
            Assert.Equal(ErrorCode.NOT_FOUND, result.Errors[0].ErrorCode);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenGetByIdReturnsErrors()
        {
            // Arrange
            var updateCommand = new InstrumentUpdateCommand("test-id", "Guitarra Actualizada", "Descripción actualizada", InstrumentType.Stringed);

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(updateCommand.Name))
               .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync("test-id"))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                    }
                });

            var useCase = new InstrumentUseCase(
               _instrumentsRepositoryPortMock.Object,
               _updateValidatorMock.Object,
               _createValidatorMock.Object,
               _deleteMassiveValidatorMock.Object,
               _loggerMock.Object,
               _unitOfWorkMock.Object,
               _domainEventsCollectorMock.Object,
               _cachePortMock.Object
               );

            // Act
            var result = await useCase.UpdateAsync(updateCommand);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenSaveChangesAsyncReturnsErrors()
        {
            // Arrange
            var existingInstrument = Instrument.Create("Guitarra Original", "Descripción original", InstrumentType.Stringed, 500, 10).Result;
            var updateCommand = new InstrumentUpdateCommand(existingInstrument.Id, "Guitarra Actualizada", "Descripción actualizada", InstrumentType.Stringed);
            int currentStock = 10;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetStockByType(It.IsAny<InstrumentType>()))
              .ReturnsAsync(new Results<int> { Result = currentStock });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByNameAsync(updateCommand.Name))
               .ReturnsAsync(new Results<Instrument?> { Result = null });

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync(existingInstrument.Id))
                .ReturnsAsync(new Results<Instrument?> { Result = existingInstrument });

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync<string>(It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Results<bool>
               {
                   Result = false,
                   Errors = new List<TiendaMusicaError>
                   {
                       new TiendaMusicaError(ErrorCode.DATABASE_ERROR, "Error al guardar los cambios en la base de datos")
                   }
               });

            var useCase = new InstrumentUseCase(
              _instrumentsRepositoryPortMock.Object,
              _updateValidatorMock.Object,
              _createValidatorMock.Object,
              _deleteMassiveValidatorMock.Object,
              _loggerMock.Object,
              _unitOfWorkMock.Object,
              _domainEventsCollectorMock.Object,
              _cachePortMock.Object
              );

            // Act
            var result = await useCase.UpdateAsync(updateCommand);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }
    }
}
