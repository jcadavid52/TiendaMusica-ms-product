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

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsSortDirectionAsc_WhenUseCaseReturnsData()
        {
            // Arrange
            var expectedInstruments = new List<Instrument>
            {
                Instrument.Create("Guitarra eléctrica", "Guitarra eléctrica description test", InstrumentType.Stringed,500,10).Result,
                Instrument.Create("Piano", "Piano description test", InstrumentType.keyboard,1000,5).Result,
                Instrument.Create("Saxofón", "Saxofón description test", InstrumentType.Wind,800,8).Result
            };

            _instrumentUseCaseMock.Setup(useCase => useCase.GetAllAsync(It.IsAny<GetAllInstrumentQuery>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = expectedInstruments.OrderBy(i => i.CreationDateUtc).ToList()
                });

            var query = new GetAllInstrumentQuery(SortDirection.Asc, null, 10, 1);

            // Act
            await _instrumentsCommand.GetAllAsync(query);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.GetAllAsync(
                It.Is<GetAllInstrumentQuery>(q =>
                    q.SortDirection == SortDirection.Asc &&
                    q.PageSize == 10 &&
                    q.PageNumber == 1
                )), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsSortDirectionDesc_WhenUseCaseReturnsData()
        {
            // Arrange
            var expectedInstruments = new List<Instrument>
            {
                Instrument.Create("Guitarra eléctrica", "Guitarra eléctrica description test", InstrumentType.Stringed,500,10).Result,
                Instrument.Create("Piano", "Piano description test", InstrumentType.keyboard,1000,5).Result,
                Instrument.Create("Saxofón", "Saxofón description test", InstrumentType.Wind,800,8).Result
            };

            _instrumentUseCaseMock.Setup(useCase => useCase.GetAllAsync(It.IsAny<GetAllInstrumentQuery>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = expectedInstruments.OrderByDescending(i => i.CreationDateUtc).ToList()
                });

            var query = new GetAllInstrumentQuery(SortDirection.Desc, null, 10, 1);

            // Act
            await _instrumentsCommand.GetAllAsync(query);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.GetAllAsync(
                It.Is<GetAllInstrumentQuery>(q =>
                    q.SortDirection == SortDirection.Desc &&
                    q.PageSize == 10 &&
                    q.PageNumber == 1
                )), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsWithSearch_WhenSearchTermIsProvided()
        {
            // Arrange
            var expectedInstruments = new List<Instrument>
            {
                Instrument.Create("Guitarra eléctrica", "Guitarra descripción test", InstrumentType.Stringed,500,10).Result
            };

            _instrumentUseCaseMock.Setup(useCase => useCase.GetAllAsync(It.IsAny<GetAllInstrumentQuery>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = expectedInstruments
                });

            var query = new GetAllInstrumentQuery(SortDirection.Desc, "Guitarra", 10, 1);

            // Act
            await _instrumentsCommand.GetAllAsync(query);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.GetAllAsync(
                It.Is<GetAllInstrumentQuery>(q =>
                    q.Search == "Guitarra" &&
                    q.SortDirection == SortDirection.Desc
                )), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsPaginated_WhenPageParametersAreProvided()
        {
            // Arrange
            var expectedInstruments = new List<Instrument>();
            for (int i = 1; i <= 5; i++)
            {
                expectedInstruments.Add(Instrument.Create($"Instrumento {i}", $"Descripción {i}", InstrumentType.Stringed, 100 * i, i).Result);
            }

            _instrumentUseCaseMock.Setup(useCase => useCase.GetAllAsync(It.IsAny<GetAllInstrumentQuery>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Result = expectedInstruments
                });

            var query = new GetAllInstrumentQuery(SortDirection.Desc, null, 5, 2);

            // Act
            await _instrumentsCommand.GetAllAsync(query);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.GetAllAsync(
                It.Is<GetAllInstrumentQuery>(q =>
                    q.PageSize == 5 &&
                    q.PageNumber == 2
                )), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldHandleErrors_WhenUseCaseReturnsErrors()
        {
            // Arrange
            _instrumentUseCaseMock.Setup(useCase => useCase.GetAllAsync(It.IsAny<GetAllInstrumentQuery>()))
                .ReturnsAsync(new Results<IList<Instrument>>
                {
                    Errors = new List<TiendaMusicaError>
                    {
                        new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                    }
                });

            var query = new GetAllInstrumentQuery();

            // Act
            await _instrumentsCommand.GetAllAsync(query);

            // Assert
            _instrumentUseCaseMock.Verify(useCase => useCase.GetAllAsync(It.IsAny<GetAllInstrumentQuery>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateInstrument_WhenUseCaseReturnsSuccess()
        {
            // Arrange
            var cliRequest = new InstrumentCreateCliRequest("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);
            var createCommand = new CreateInstrumentCommand("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);
            var createdInstrument = Instrument.Create("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1).Result;

            _mapperMock.Setup(m => m.Map<CreateInstrumentCommand>(cliRequest))
                .Returns(createCommand);

            _instrumentUseCaseMock.Setup(useCase => useCase.CreateAsync(It.IsAny<CreateInstrumentCommand>()))
                .ReturnsAsync(new Results<Instrument>
                {
                    Result = createdInstrument
                });

            // Act
            await _instrumentsCommand.CreateAsync(cliRequest);

            // Assert
            _mapperMock.Verify(m => m.Map<CreateInstrumentCommand>(cliRequest), Times.Once);
            _instrumentUseCaseMock.Verify(useCase => useCase.CreateAsync(
                It.Is<CreateInstrumentCommand>(cmd =>
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
            var createCommand = new CreateInstrumentCommand("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);

            _mapperMock.Setup(m => m.Map<CreateInstrumentCommand>(cliRequest))
                .Returns(createCommand);

            _instrumentUseCaseMock.Setup(useCase => useCase.CreateAsync(It.IsAny<CreateInstrumentCommand>()))
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
            _instrumentUseCaseMock.Verify(useCase => useCase.CreateAsync(It.IsAny<CreateInstrumentCommand>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldHandleConflictError_WhenInstrumentAlreadyExists()
        {
            // Arrange
            var cliRequest = new InstrumentCreateCliRequest("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);
            var createCommand = new CreateInstrumentCommand("Guitarra Eléctrica", "Descripción test", InstrumentType.Stringed, 1500.00m, 1);

            _mapperMock.Setup(m => m.Map<CreateInstrumentCommand>(cliRequest))
                .Returns(createCommand);

            _instrumentUseCaseMock.Setup(useCase => useCase.CreateAsync(It.IsAny<CreateInstrumentCommand>()))
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
                It.Is<CreateInstrumentCommand>(cmd => cmd.Name == "Guitarra Eléctrica")), Times.Once);
        }
    }
}
