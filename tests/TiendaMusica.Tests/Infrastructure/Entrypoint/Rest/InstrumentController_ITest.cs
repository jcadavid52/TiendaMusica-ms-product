using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Collections;
using System.Text;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Domain.Enums;
using TiendaMusica.Domain.Models;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Infrastructure.Entrypoint.Rest.Dtos;

namespace TiendaMusica.Tests.Infrastructure.Entrypoint.Rest
{
    public class InstrumentController_ITest : IClassFixture<WebAppTestFactory>
    {
        private readonly HttpClient _client;
        private readonly WebAppTestFactory _factory;

        public InstrumentController_ITest(WebAppTestFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsSortDirectionAsc_WhenUseCaseReturnsDataStatusCode200()
        {
            //Arrange
            var instruments = await _factory.ScopedDatabaseAsync();
            string url = "/v1/instrument?sortDirection=Asc";

            //Act
            var client = _factory.CreateClient();
            var response = await client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseString);

            //Assert
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal(instruments.Count, responseObject.Result.Count);
            Assert.Equal(instruments[0].Name, responseObject.Result[9].Name);
            Assert.Equal(instruments[1].Name, responseObject.Result[8].Name);
            Assert.Equal(instruments[2].Name, responseObject.Result[7].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsSortDirectionDesc_WhenUseCaseReturnsDataStatusCode200()
        {
            //Arrange
            var instruments = await _factory.ScopedDatabaseAsync();

            string url = "/v1/instrument?sortDirection=Desc";

            //Act
            var client = _factory.CreateClient();
            var response = await client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseString);

            //Assert
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal(instruments.Count, responseObject.Result.Count);
            Assert.Equal(instruments[9].Name, responseObject.Result[9].Name);
            Assert.Equal(instruments[8].Name, responseObject.Result[8].Name);
            Assert.Equal(instruments[7].Name, responseObject.Result[7].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsFilteredBySearch_WhenSearchTermMatches()
        {
            //Arrange
            var instruments = await _factory.ScopedDatabaseAsync();
            string searchTerm = instruments[0].Name;
            string url = $"/v1/instrument?search={searchTerm}";

            //Act
            var client = _factory.CreateClient();
            var response = await client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseString);

            //Assert
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Single(responseObject.Result);
            Assert.Equal(instruments[0].Name, responseObject.Result[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsPaginated_WhenPageParametersAreProvided()
        {
            //Arrange
            var instruments = await _factory.ScopedDatabaseAsync();

            string urlPage1 = "/v1/instrument?pageNumber=1&pageSize=5&sortDirection=Desc";

            //Act - Page 1 (most recent first with Desc)
            var client = _factory.CreateClient();
            var responsePage1 = await client.GetAsync(urlPage1);
            var responseStringPage1 = await responsePage1.Content.ReadAsStringAsync();
            var responseObjectPage1 = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseStringPage1);

            //Assert - Page 1
            Assert.NotNull(responseObjectPage1);
            Assert.NotNull(responseObjectPage1.Result);
            Assert.True(responseObjectPage1.IsSuccess);
            Assert.Equal(5, responseObjectPage1.Result.Count);
            Assert.Equal(instruments[0].Name, responseObjectPage1.Result[0].Name);
            Assert.Equal(instruments[4].Name, responseObjectPage1.Result[4].Name);

            //Act - Page 2
            string urlPage2 = "/v1/instrument?pageNumber=2&pageSize=5&sortDirection=Desc";
            var responsePage2 = await client.GetAsync(urlPage2);
            var responseStringPage2 = await responsePage2.Content.ReadAsStringAsync();
            var responseObjectPage2 = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseStringPage2);

            //Assert - Page 2
            Assert.NotNull(responseObjectPage2);
            Assert.NotNull(responseObjectPage2.Result);
            Assert.True(responseObjectPage2.IsSuccess);
            Assert.Equal(5, responseObjectPage2.Result.Count);
            Assert.Equal(instruments[5].Name, responseObjectPage2.Result[0].Name);
            Assert.Equal(instruments[9].Name, responseObjectPage2.Result[4].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstruments_WhenUseCaseReturnsDataStatusCode200()
        {
            // Arrange
            var url = "/v1/instrument";
            int codeExpected = 200;

            // Act
            var response = await _client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseString);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetAllAsync_WhenUseCaseReturnsErrors_ShouldReturnsFailureResultStatus500()
        {
            // Arrange
            var url = "/v1/instrument";
            int codeExpected = 500;

            // Act
            var errorClient = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IInstrumentUseCase));
                    if (descriptor != null) services.Remove(descriptor);

                    var mockInstrumentUsecase = new Mock<IInstrumentUseCase>();
                    mockInstrumentUsecase.Setup(m => m.GetAllAsync(null))
                                      .ReturnsAsync(new Results<IList<Instrument>>
                                      {
                                          Errors = new List<TiendaMusicaError>
                                          {
                                              new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                                          },
                                      });

                    services.AddScoped(_ => mockInstrumentUsecase.Object);
                });
            }).CreateClient();

            var response = await errorClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseString);

            // Assert
            Assert.True(responseObject!.Errors.Any());
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.Null(responseObject.Result);
            Assert.False(responseObject.IsSuccess);
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_WhenUseCaseCreateSuccess_ShouldResturnSuccessResultStatus201()
        {
            // Arrange
            var url = "/v1/instrument";
            int codeExpected = 201;

            var newInstrument = new InstrumentRequest("Batería Eléctrica",
                "Descripción test",
                InstrumentType.Percussion,
                1500.00m,
                1
                );
            var content = new StringContent(JsonConvert.SerializeObject(newInstrument), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<InstrumentResponse>>(responseString);
            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal(newInstrument.Name, responseObject.Result.Name);
            Assert.Equal(newInstrument.Description, responseObject.Result.Description);
            Assert.Equal(newInstrument.Type, responseObject.Result.Type);
            Assert.Equal(newInstrument.Price, responseObject.Result.Price);
            Assert.Equal(newInstrument.Stock, responseObject.Result.Stock);
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Theory]
        [ClassData(typeof(ValidationTestData))]
        public async Task CreateAsync_WhenRequestContaintsError_ShouldReturnFailureResultStatus400(InstrumentRequest request, Results<InstrumentResponse> expected)
        {
            // Arrange
            var url = "/v1/instrument";
            int codeExpected = 400;
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<InstrumentResponse>>(responseString);
            // Assert
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.Null(responseObject.Result);
            Assert.False(responseObject.IsSuccess);
            Assert.True(responseObject.Errors.Any());
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_WhenUseCaseReturnsErrors_ShouldReturnsFailureResultStatus500()
        {
            // Arrange
            var url = "/v1/instrument";
            int codeExpected = 500;
            var newInstrument = new InstrumentRequest("Guitarra Eléctrica",
                "Descripción test",
                InstrumentType.Stringed,
                1500.00m,
                1
                );
            var content = new StringContent(JsonConvert.SerializeObject(newInstrument), Encoding.UTF8, "application/json");

            // Act
            var errorClient = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IInstrumentUseCase));
                    if (descriptor != null) services.Remove(descriptor);

                    var mockInstrumentUsecase = new Mock<IInstrumentUseCase>();
                    mockInstrumentUsecase.Setup(m => m.CreateAsync(It.IsAny<CreateInstrumentCommand>()))
                                      .ReturnsAsync(new Results<Instrument>
                                      {
                                          Errors = new List<TiendaMusicaError>
                                          {
                                              new TiendaMusicaError(ErrorCode.SERVER_ERROR,"Error en el servidor")
                                          },
                                      });

                    services.AddScoped(_ => mockInstrumentUsecase.Object);
                });
            }).CreateClient();

            var response = await errorClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<InstrumentResponse>>(responseString);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.Null(responseObject.Result);
            Assert.False(responseObject.IsSuccess);
            Assert.True(responseObject.Errors.Any());
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenInstrumentExists_ShouldResturnSuccessResultStatus200()
        {
            // Arrange
            var instruments = await _factory.ScopedDatabaseAsync();
            string url = "/v1/instrument?sortDirection=Desc";
            var response = await _client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var allInstruments = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseString);

            var instrumentId = allInstruments?.Result?.FirstOrDefault()?.Id;
            url = $"/v1/instrument/{instrumentId}";
            int codeExpected = 200;

            // Act
            response = await _client.GetAsync(url);
            responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<InstrumentResponse>>(responseString);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal(instruments[0].Name, responseObject.Result.Name);
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenInstrumentNotExists_ShouldResturnFailureResultStatus404()
        {
            // Arrange
            string url = $"/v1/instrument/non-existent-id";
            int codeExpected = 404;

            // Act
            var response = await _client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<InstrumentResponse>>(responseString);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.Null(responseObject.Result);
            Assert.False(responseObject.IsSuccess);
            Assert.True(responseObject.Errors.Any());
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenUseCaseReturnsErrors_ShouldReturnsFailureResultStatus500()
        {
            // Arrange
            string url = $"/v1/instrument/test-id";
            int codeExpected = 500;

            var errorClient = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IInstrumentUseCase));
                    if (descriptor != null) services.Remove(descriptor);

                    var mockInstrumentUsecase = new Mock<IInstrumentUseCase>();
                    mockInstrumentUsecase.Setup(m => m.GetByIdAsync(It.IsAny<string>()))
                                      .ReturnsAsync(new Results<Instrument?>
                                      {
                                          Errors = new List<TiendaMusicaError>
                                          {
                                              new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                                          },
                                      });

                    services.AddScoped(_ => mockInstrumentUsecase.Object);
                });
            }).CreateClient();

            // Act
            var response = await errorClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<InstrumentResponse>>(responseString);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.Null(responseObject.Result);
            Assert.False(responseObject.IsSuccess);
            Assert.True(responseObject.Errors.Any());
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }


        private class ValidationTestData : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                 new object[]
                 {
                    new InstrumentRequest("","Descripción test",InstrumentType.Stringed,1500.00m,1),
                    new Results<InstrumentResponse>
                    {
                        Errors = new List<TiendaMusicaError>
                        {
                            new TiendaMusicaError(ErrorCode.VALIDATION_ERROR, "Error en la validación del request")
                        }
                    }
                 },
                 new object[]
                 {
                    new InstrumentRequest("Nombre instrumento","",InstrumentType.Stringed,1500.00m,1),
                    new Results<InstrumentResponse>
                    {
                        Errors = new List<TiendaMusicaError>
                        {
                            new TiendaMusicaError(ErrorCode.VALIDATION_ERROR, "Error en la validación del request")
                        }
                    }
                 },
                 new object[]
                 {
                    new InstrumentRequest("Nombre instrumento","Descripción test",InstrumentType.Stringed,0,1),
                    new Results<InstrumentResponse>
                    {
                        Errors = new List<TiendaMusicaError>
                        {
                            new TiendaMusicaError(ErrorCode.VALIDATION_ERROR, "Error en la validación del request")
                        }
                    }
                 },
                 new object[]
                 {
                    new InstrumentRequest("Nombre instrumento","Descripción test",InstrumentType.Stringed,900m,-1),
                    new Results<InstrumentResponse>
                    {
                        Errors = new List<TiendaMusicaError>
                        {
                            new TiendaMusicaError(ErrorCode.VALIDATION_ERROR, "Error en la validación del request")
                        }
                    }
                 }
            };

            public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
