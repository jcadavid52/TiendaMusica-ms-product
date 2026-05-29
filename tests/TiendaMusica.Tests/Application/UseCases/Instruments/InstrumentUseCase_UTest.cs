using Microsoft.Extensions.Logging;
using Moq;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Application.UseCases.Instruments.Validators;
using TiendaMusica.Domain.Dtos;
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
        private readonly Mock<IGenericValidator<InstrumentUpdateCommand, Instrument>> _updateValidatorMock;
        private readonly Mock<IGenericValidator<InstrumentCreateCommand, bool>> _createValidatorMock;
        private readonly Mock<IGenericValidator<InstrumentDeleteMultipleCommand, IList<Instrument>>> _deleteMassiveValidatorMock;
        private readonly InstrumentUseCase instrumentUseCase;

        public InstrumentUseCase_UTest()
        {
            _instrumentsRepositoryPortMock = new Mock<IInstrumentsRepositoryPort>();
            _loggerMock = new Mock<ILogger<InstrumentUseCase>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _domainEventsCollectorMock = new Mock<DomainEventsCollector>();
            _updateValidatorMock = new Mock<IGenericValidator<InstrumentUpdateCommand, Instrument>>();
            _createValidatorMock = new Mock<IGenericValidator<InstrumentCreateCommand, bool>>();
            _deleteMassiveValidatorMock = new Mock<IGenericValidator<InstrumentDeleteMultipleCommand, IList<Instrument>>>();

            instrumentUseCase = new InstrumentUseCase(
                _instrumentsRepositoryPortMock.Object,
                _updateValidatorMock.Object,
                _createValidatorMock.Object,
                _deleteMassiveValidatorMock.Object,
                _loggerMock.Object,
                _unitOfWorkMock.Object,
                _domainEventsCollectorMock.Object
                );
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

            _instrumentsRepositoryPortMock.Setup(m => m.GetAllAsync(It.IsAny<InstrumentGetAllQueryParametersDto>()))
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
                _domainEventsCollectorMock.Object
                );

            // Act
            var result = await useCase.GetAllAsync(null);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasErrors);
            Assert.Equal(expectedInstruments.Count, result.Result.Count);
            Assert.Equal(expectedInstruments[0].Name, result.Result[0].Name);
            Assert.Equal(expectedInstruments[1].Name, result.Result[1].Name);
        }

        [Fact]
        public async Task GetAllAsync_WhenRepositoryReturnsErrors_ReturnsFailureResult()
        {
            // Arrange
            _instrumentsRepositoryPortMock.Setup(repo => repo.GetAllAsync(It.IsAny<InstrumentGetAllQueryParametersDto>()))
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
                _domainEventsCollectorMock.Object
                );

            // Act
            var result = await useCase.GetAllAsync(null);
            // Assert
            Assert.Null(result.Result);
            Assert.True(result.HasErrors);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnsFailureResult_WhenExistingInstrument()
        {
            // Arrange
            var createCommand = new InstrumentCreateCommand(
                "Instrument test",
                "Instrument test description",
                InstrumentType.Stringed,
                1500,
                1);

            _createValidatorMock.Setup(validator => validator.ValidateAsync(It.IsAny<InstrumentCreateCommand>()))
                                       .ReturnsAsync(new Results<bool> { Result = false }.AddError(ErrorCode.CONFLICT_ERROR, $"Ya existe: '{createCommand.Name}'"));

            // Act
            var result = await instrumentUseCase.CreateAsync(createCommand);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
            Assert.Equal(ErrorCode.CONFLICT_ERROR, result.Errors[0].ErrorCode);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnsFailureResult_WhenRepositoryReturnsErrors()
        {
            // Arrange
            var createCommand = new InstrumentCreateCommand(
                "Instrument test",
                "Instrument test description",
                InstrumentType.Stringed,
                1500,
                1);

            _createValidatorMock.Setup(validator => validator.ValidateAsync(It.IsAny<InstrumentCreateCommand>()))
                            .ReturnsAsync(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                         new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                    }
                });

            // Act
            var result = await instrumentUseCase.CreateAsync(createCommand);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnsFailureResult_WhenRepositoryThrowArgumentException()
        {
            // Arrange
            var createCommand = new InstrumentCreateCommand(
                "Instrument test",
                "Instrument test description",
                InstrumentType.Stringed,
                1500,
                1);

            _createValidatorMock.Setup(validator => validator.ValidateAsync(It.IsAny<InstrumentCreateCommand>()))
                           .ReturnsAsync(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .Throws(new ArgumentException("Exception forzada en el UseCase"));

            // Act
            var result = await instrumentUseCase.CreateAsync(createCommand);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateInstrument_WhenValidationPasses()
        {
            //Assert
            var createCommand = new InstrumentCreateCommand(
                "Instrument test",
                "Instrument test description",
                InstrumentType.Stringed,
                1500,
                1);

            bool expectedChanges = true;

            //Act
            _createValidatorMock.Setup(validator => validator.ValidateAsync(It.IsAny<InstrumentCreateCommand>()))
                .ReturnsAsync(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = Instrument.Create(
                        createCommand.Name,
                        createCommand.Description,
                        createCommand.Type,
                        createCommand.Price,
                        createCommand.Stock
                    ).Result
                });

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync<string>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Results<bool>
                {
                    Result = expectedChanges
                });

            var result = await instrumentUseCase.CreateAsync(createCommand);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.HasErrors);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowsException_WhenSaveChangesThrowsException()
        {
            var createCommand = new InstrumentCreateCommand(
                "Instrument test",
                "Instrument test description",
                InstrumentType.Stringed,
                1500,
                1
            );

            _createValidatorMock.Setup(validator => validator.ValidateAsync(It.IsAny<InstrumentCreateCommand>()))
                           .ReturnsAsync(new Results<bool> { Result = true });

            _instrumentsRepositoryPortMock.Setup(repo => repo.CreateAsync(It.IsAny<Instrument>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = Instrument.Create(
                        createCommand.Name,
                        createCommand.Description,
                        createCommand.Type,
                        createCommand.Price,
                        createCommand.Stock
                    ).Result
                });

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync<string>(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error al guardar los cambios en la base de datos"));

            await Assert.ThrowsAsync<Exception>(() => instrumentUseCase.CreateAsync(createCommand));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnInstrument_WhenInstrumentExists()
        {
            // Arrange
            var expectedInstrument = Instrument.Create(
                "Guitarra eléctrica",
                "Guitarra eléctrica description test",
                InstrumentType.Stringed,
                500,
                10
            ).Result;

            _instrumentsRepositoryPortMock.Setup(repo => repo.GetByIdAsync(expectedInstrument.Id))
                .ReturnsAsync(new Results<Instrument?> { Result = expectedInstrument });

            // Act
            var result = await instrumentUseCase.GetByIdAsync(expectedInstrument.Id);

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

            // Act
            var result = await instrumentUseCase.GetByIdAsync(id);

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

            // Act
            var result = await instrumentUseCase.GetByIdAsync(id);

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
            var existingInstrument = Instrument.Create(
                "Guitarra Original",
                "Descripción original",
                InstrumentType.Stringed,
                500,
                10).Result;
            var updateCommand = new InstrumentUpdateCommand(existingInstrument.Id, "Guitarra Actualizada", "Descripción actualizada", InstrumentType.Stringed);
            bool expectedChanges = true;

            _updateValidatorMock.Setup(validator => validator.ValidateAsync(It.IsAny<InstrumentUpdateCommand>()))
                            .ReturnsAsync(new Results<Instrument> { Result = existingInstrument });

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync<string>(It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Results<bool>
               {
                   Result = expectedChanges
               });

            // Act
            var result = await instrumentUseCase.UpdateAsync(updateCommand);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.HasErrors);
            Assert.Equal(existingInstrument.Id, result.Result.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnError_WhenSaveChangesAsyncReturnsErrors()
        {
            // Arrange
            var existingInstrument = Instrument.Create(
                "Guitarra Original",
                "Descripción original",
                InstrumentType.Stringed,
                500,
                10).Result;
            var updateCommand = new InstrumentUpdateCommand(existingInstrument.Id, "Guitarra Actualizada", "Descripción actualizada", InstrumentType.Stringed);

            _updateValidatorMock.Setup(validator => validator.ValidateAsync(It.IsAny<InstrumentUpdateCommand>()))
                .ReturnsAsync(new Results<Instrument> { Result = existingInstrument });

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync<string>(It.IsAny<CancellationToken>()))
               .ReturnsAsync(new Results<bool>
               {
                   Result = false,
                   Errors = new List<TiendaMusicaError>
                   {
                       new TiendaMusicaError(ErrorCode.DATABASE_ERROR, "Error al guardar los cambios en la base de datos")
                   }
               });

            // Act
            var result = await instrumentUseCase.UpdateAsync(updateCommand);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.HasErrors);
        }
    }
}
