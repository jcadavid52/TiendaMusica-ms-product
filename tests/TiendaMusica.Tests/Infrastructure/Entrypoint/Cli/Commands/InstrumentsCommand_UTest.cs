using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Commands;
using TiendaMusica.Infrastructure.Entrypoint.Cli.Dtos;
using TiendaMusica.Utilities;

namespace TiendaMusica.Tests.Infrastructure.Entrypoint.Cli.Commands
{
    public class InstrumentsCommand_UTest
    {
        private readonly Mock<ILogger<InstrumentsCommand>> _loggerMock;
        private readonly Mock<IInstrumentUseCase> _instrumentUseCaseMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly InstrumentsCommand _instrumentsCommand;
        private readonly Mock<ITools> _toolsMock;

        public InstrumentsCommand_UTest()
        {
            _loggerMock = new Mock<ILogger<InstrumentsCommand>>();
            _instrumentUseCaseMock = new Mock<IInstrumentUseCase>();
            _mapperMock = new Mock<IMapper>();
            _toolsMock = new Mock<ITools>();
            _instrumentsCommand = new InstrumentsCommand(
                _instrumentUseCaseMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _toolsMock.Object
                );
        }

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnInstrumentsSortDirectionAsc_WhenUseCaseReturnsData()
        //{
        //    // Arrange
        //    var expectedInstruments = new List<Instrument>
        //    {
        //        Instrument.Create("Guitarra eléctrica", "Guitarra eléctrica description test", InstrumentType.Stringed,500,10).Result,
        //        Instrument.Create("Piano", "Piano description test", InstrumentType.keyboard,1000,5).Result,
        //        Instrument.Create("Saxofón", "Saxofón description test", InstrumentType.Wind,800,8).Result
        //    };

        //    _instrumentUseCaseMock.Setup(useCase => useCase.GetAllAsync(It.IsAny<InstrumentGetAllQuery>()))
        //        .ReturnsAsync(new Results<IList<Instrument>>
        //        {
        //            Result = expectedInstruments.OrderBy(i => i.CreationDateUtc).ToList()
        //        });

        //    var query = new InstrumentGetAllQuery(SortDirection.Asc, null, 10, 1);

        //    // Act
        //    await _instrumentsCommand.GetAllAsync(query);

        //    // Assert
        //    _instrumentUseCaseMock.Verify(useCase => useCase.GetAllAsync(
        //        It.Is<InstrumentGetAllQuery>(q =>
        //            q.SortDirection == SortDirection.Asc &&
        //            q.PageSize == 10 &&
        //            q.PageNumber == 1
        //        )), Times.Once);
        //}

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnInstrumentsSortDirectionDesc_WhenUseCaseReturnsData()
        //{
        //    // Arrange
        //    var expectedInstruments = new List<Instrument>
        //    {
        //        Instrument.Create("Guitarra eléctrica", "Guitarra eléctrica description test", InstrumentType.Stringed,500,10).Result,
        //        Instrument.Create("Piano", "Piano description test", InstrumentType.keyboard,1000,5).Result,
        //        Instrument.Create("Saxofón", "Saxofón description test", InstrumentType.Wind,800,8).Result
        //    };

        //    _instrumentUseCaseMock.Setup(useCase => useCase.GetAllAsync(It.IsAny<InstrumentGetAllQuery>()))
        //        .ReturnsAsync(new Results<IList<Instrument>>
        //        {
        //            Result = expectedInstruments.OrderByDescending(i => i.CreationDateUtc).ToList()
        //        });

        //    var query = new InstrumentGetAllQuery(SortDirection.Desc, null, 10, 1);

        //    // Act
        //    await _instrumentsCommand.GetAllAsync(query);

        //    // Assert
        //    _instrumentUseCaseMock.Verify(useCase => useCase.GetAllAsync(
        //        It.Is<InstrumentGetAllQuery>(q =>
        //            q.SortDirection == SortDirection.Desc &&
        //            q.PageSize == 10 &&
        //            q.PageNumber == 1
        //        )), Times.Once);
        //}

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnInstrumentsWithSearch_WhenSearchTermIsProvided()
        //{
        //    // Arrange
        //    var expectedInstruments = new List<Instrument>
        //    {
        //        Instrument.Create("Guitarra eléctrica", "Guitarra descripción test", InstrumentType.Stringed,500,10).Result
        //    };

        //    _instrumentUseCaseMock.Setup(useCase => useCase.GetAllAsync(It.IsAny<InstrumentGetAllQuery>()))
        //        .ReturnsAsync(new Results<IList<Instrument>>
        //        {
        //            Result = expectedInstruments
        //        });

        //    var query = new InstrumentGetAllQuery(SortDirection.Desc, "Guitarra", 10, 1);

        //    // Act
        //    await _instrumentsCommand.GetAllAsync(query);

        //    // Assert
        //    _instrumentUseCaseMock.Verify(useCase => useCase.GetAllAsync(
        //        It.Is<InstrumentGetAllQuery>(q =>
        //            q.Search == "Guitarra" &&
        //            q.SortDirection == SortDirection.Desc
        //        )), Times.Once);
        //}

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnInstrumentsPaginated_WhenPageParametersAreProvided()
        //{
        //    // Arrange
        //    var expectedInstruments = new List<Instrument>();
        //    for (int i = 1; i <= 5; i++)
        //    {
        //        expectedInstruments.Add(Instrument.Create($"Instrumento {i}", $"Descripción {i}", InstrumentType.Stringed, 100 * i, i).Result);
        //    }

        //    _instrumentUseCaseMock.Setup(useCase => useCase.GetAllAsync(It.IsAny<InstrumentGetAllQuery>()))
        //        .ReturnsAsync(new Results<IList<Instrument>>
        //        {
        //            Result = expectedInstruments
        //        });

        //    var query = new InstrumentGetAllQuery(SortDirection.Desc, null, 5, 2);

        //    // Act
        //    await _instrumentsCommand.GetAllAsync(query);

        //    // Assert
        //    _instrumentUseCaseMock.Verify(useCase => useCase.GetAllAsync(
        //        It.Is<InstrumentGetAllQuery>(q =>
        //            q.PageSize == 5 &&
        //            q.PageNumber == 2
        //        )), Times.Once);
        //}

        //[Fact]
        //public async Task GetAllAsync_ShouldHandleErrors_WhenUseCaseReturnsErrors()
        //{
        //    // Arrange
        //    _instrumentUseCaseMock.Setup(useCase => useCase.GetAllAsync(It.IsAny<InstrumentGetAllQuery>()))
        //        .ReturnsAsync(new Results<IList<Instrument>>
        //        {
        //            Errors = new List<TiendaMusicaError>
        //            {
        //                new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
        //            }
        //        });

        //    var query = new InstrumentGetAllQuery();

        //    // Act
        //    await _instrumentsCommand.GetAllAsync(query);

        //    // Assert
        //    _instrumentUseCaseMock.Verify(useCase => useCase.GetAllAsync(It.IsAny<InstrumentGetAllQuery>()), Times.Once);
        //}

        [Fact]
        public async Task CreateAsync_ShouldCreateInstrument_WhenUseCaseReturnsSuccess()
        {
            // Arrange
            var cliRequest = new InstrumentCreateCliRequest("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);
            var createCommand = new InstrumentCreateCommand("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);
            var createdInstrument = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1).Result;

            _mapperMock.Setup(m => m.Map<InstrumentCreateCommand>(cliRequest))
                .Returns(createCommand);

            _instrumentUseCaseMock.Setup(useCase => useCase.CreateAsync(It.IsAny<InstrumentCreateCommand>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = createdInstrument
                });

            // Act
            await _instrumentsCommand.CreateAsync(cliRequest);

            // Assert
            _mapperMock.Verify(m => m.Map<InstrumentCreateCommand>(cliRequest), Times.Once);
            _instrumentUseCaseMock.Verify(useCase => useCase.CreateAsync(
                It.Is<InstrumentCreateCommand>(cmd =>
                    cmd.Name == "Guitarra Eléctrica" &&
                    cmd.Description == "Descripción test" &&
                    cmd.Type == InstrumentType.Stringed &&
                    cmd.Price == 1500.00m &&
                    cmd.Stock == 1
                )), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldHandleErrors_WhenUseCaseReturnsErrors()
        {
            // Arrange
            var cliRequest = new InstrumentCreateCliRequest("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);
            var createCommand = new InstrumentCreateCommand("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);

            _mapperMock.Setup(m => m.Map<InstrumentCreateCommand>(cliRequest))
                .Returns(createCommand);

            _instrumentUseCaseMock.Setup(useCase => useCase.CreateAsync(It.IsAny<InstrumentCreateCommand>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                    }
                });

            // Act
            await _instrumentsCommand.CreateAsync(cliRequest);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.CreateAsync(It.IsAny<InstrumentCreateCommand>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldHandleConflictError_WhenInstrumentAlreadyExists()
        {
            // Arrange
            var cliRequest = new InstrumentCreateCliRequest("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);
            var createCommand = new InstrumentCreateCommand("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);

            _mapperMock.Setup(m => m.Map<InstrumentCreateCommand>(cliRequest))
                .Returns(createCommand);

            _instrumentUseCaseMock.Setup(useCase => useCase.CreateAsync(It.IsAny<InstrumentCreateCommand>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.CONFLICT_ERROR, "Ya existe: 'Guitarra Eléctrica'")
                    }
                });

            // Act
            await _instrumentsCommand.CreateAsync(cliRequest);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.CreateAsync(
                It.Is<InstrumentCreateCommand>(cmd => cmd.Name == "Guitarra Eléctrica")), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnInstrument_WhenInstrumentExists()
        {
            // Arrange
            var instrument = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1).Result;

            _instrumentUseCaseMock.Setup(useCase => useCase.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = instrument
                });

            // Act
            await _instrumentsCommand.GetByIdAsync(instrument.Id);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.GetByIdAsync(
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldHandleErrors_WhenUseCaseReturnsErrors()
        {
            // Arrange
            var instrumentId = "test-id";

            _instrumentUseCaseMock.Setup(useCase => useCase.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                    }
                });

            // Act
            await _instrumentsCommand.GetByIdAsync(instrumentId);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.GetByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldHandleNotFound_WhenInstrumentNotExists()
        {
            // Arrange
            var instrumentId = "non-existent-id";

            _instrumentUseCaseMock.Setup(useCase => useCase.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Results<Instrument?>
                {
                    Result = null,
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.CONFLICT_ERROR, "Instrumento no encontrado")
                    }
                });

            // Act
            await _instrumentsCommand.GetByIdAsync(instrumentId);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.GetByIdAsync(
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldHandleEmptyId_WhenIdIsEmpty()
        {
            // Act
            await _instrumentsCommand.GetByIdAsync("");

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMultipleAsync_ShouldDeleteInstruments_WhenUseCaseReturnsSuccess()
        {
            // Arrange
            var idsToDelete = new List<string> { "id1", "id2", "id3" };
            var deleteCommand = new InstrumentDeleteMultipleCommand(idsToDelete);

            _instrumentUseCaseMock.Setup(useCase => useCase.DeleteMultipleAsync(It.IsAny<InstrumentDeleteMultipleCommand>()))
                .ReturnsAsync(new Results<int>
                {
                    Result = 3
                });

            // Act
            await _instrumentsCommand.DeleteMultipleAsync(idsToDelete);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.DeleteMultipleAsync(
                It.Is<InstrumentDeleteMultipleCommand>(cmd =>
                    cmd.InstrumentIds.Count == 3 &&
                    cmd.InstrumentIds[0] == "id1" &&
                    cmd.InstrumentIds[1] == "id2" &&
                    cmd.InstrumentIds[2] == "id3"
                )), Times.Once);
        }

        [Fact]
        public async Task DeleteMultipleAsync_ShouldHandleErrors_WhenUseCaseReturnsErrors()
        {
            // Arrange
            var idsToDelete = new List<string> { "id1", "id2" };

            _instrumentUseCaseMock.Setup(useCase => useCase.DeleteMultipleAsync(It.IsAny<InstrumentDeleteMultipleCommand>()))
                .ReturnsAsync(new Results<int>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                    }
                });

            // Act
            await _instrumentsCommand.DeleteMultipleAsync(idsToDelete);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.DeleteMultipleAsync(It.IsAny<InstrumentDeleteMultipleCommand>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMultipleAsync_ShouldNotCallUseCaseMethod_WhenIdListIsEmpty()
        {
            // Arrange
            var idsToDelete = new List<string>();

            _instrumentUseCaseMock.Setup(useCase => useCase.DeleteMultipleAsync(It.IsAny<InstrumentDeleteMultipleCommand>()))
                .ReturnsAsync(new Results<int>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.VALIDATION_ERROR, "La lista de IDs no puede estar vacía")
                    }
                });

            // Act
            await _instrumentsCommand.DeleteMultipleAsync(idsToDelete);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.DeleteMultipleAsync(
                It.Is<InstrumentDeleteMultipleCommand>(cmd =>
                    cmd.InstrumentIds.Count == 0
                )), Times.Never);
        }

        [Fact]
        public async Task DeleteMultipleAsync_ShouldHandlePartialFailure_WhenSomeInstrumentsNotFound()
        {
            // Arrange
            var idsToDelete = new List<string> { "id1", "non-existent-id", "id3" };

            _instrumentUseCaseMock.Setup(useCase => useCase.DeleteMultipleAsync(It.IsAny<InstrumentDeleteMultipleCommand>()))
                .ReturnsAsync(new Results<int>
                {
                    Result = 2
                });

            // Act
            await _instrumentsCommand.DeleteMultipleAsync(idsToDelete);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.DeleteMultipleAsync(
                It.Is<InstrumentDeleteMultipleCommand>(cmd =>
                    cmd.InstrumentIds.Count == 3
                )), Times.Once);
        }

        [Fact]
        public async Task DeleteMultipleAsync_ShouldHandleEmptyList_WhenNoIdsProvided()
        {
            // Act
            await _instrumentsCommand.DeleteMultipleAsync(new List<string>());

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.DeleteMultipleAsync(It.IsAny<InstrumentDeleteMultipleCommand>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateInstrument_WhenUseCaseReturnsSuccess()
        {
            // Arrange
            var cliRequest = new InstrumentUpdateCliRequest("test-id", "Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed);
            var updateCommand = new InstrumentUpdateCommand("test-id", "Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed);
            var updatedInstrument = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1).Result;

            _mapperMock.Setup(m => m.Map<InstrumentUpdateCommand>(cliRequest))
                .Returns(updateCommand);

            _instrumentUseCaseMock.Setup(useCase => useCase.UpdateAsync(It.IsAny<InstrumentUpdateCommand>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = updatedInstrument
                });

            // Act
            await _instrumentsCommand.UpdateAsync(cliRequest);

            // Assert
            _mapperMock.Verify(m => m.Map<InstrumentUpdateCommand>(cliRequest), Times.Once);
            _instrumentUseCaseMock.Verify(useCase => useCase.UpdateAsync(
                It.Is<InstrumentUpdateCommand>(cmd =>
                    cmd.Id == "test-id" &&
                    cmd.Name == "Guitarra Eléctrica" &&
                    cmd.Description == "Descripción test" &&
                    cmd.Type == InstrumentType.Stringed
                )), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldHandleErrors_WhenUseCaseReturnsErrors()
        {
            // Arrange
            var cliRequest = new InstrumentUpdateCliRequest("test-id", "Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed);
            var updateCommand = new InstrumentUpdateCommand("test-id", "Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed);

            _mapperMock.Setup(m => m.Map<InstrumentUpdateCommand>(cliRequest))
                .Returns(updateCommand);

            _instrumentUseCaseMock.Setup(useCase => useCase.UpdateAsync(It.IsAny<InstrumentUpdateCommand>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                    }
                });

            // Act
            await _instrumentsCommand.UpdateAsync(cliRequest);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.UpdateAsync(It.IsAny<InstrumentUpdateCommand>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldHandleNotFoundError_WhenInstrumentNotExists()
        {
            // Arrange
            var cliRequest = new InstrumentUpdateCliRequest("non-existent-id", "Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed);
            var updateCommand = new InstrumentUpdateCommand("non-existent-id", "Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed);

            _mapperMock.Setup(m => m.Map<InstrumentUpdateCommand>(cliRequest))
                .Returns(updateCommand);

            _instrumentUseCaseMock.Setup(useCase => useCase.UpdateAsync(It.IsAny<InstrumentUpdateCommand>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.NOT_FOUND, "Instrumento no encontrado")
                    }
                });

            // Act
            await _instrumentsCommand.UpdateAsync(cliRequest);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.UpdateAsync(
                It.Is<InstrumentUpdateCommand>(cmd => cmd.Id == "non-existent-id")), Times.Once);
        }
    }
}
