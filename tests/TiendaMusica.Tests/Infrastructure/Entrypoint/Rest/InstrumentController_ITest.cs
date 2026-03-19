using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Collections;
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
                    mockInstrumentUsecase.Setup(m => m.GetAllAsync())
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

            var newInstrument = new InstrumentRequest("Guitarra Eléctrica",
                "Descripción test",
                InstrumentType.Stringed,
                1500.00m,
                1
                );
            var content = new StringContent(JsonConvert.SerializeObject(newInstrument), System.Text.Encoding.UTF8, "application/json");

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
            var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");

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
            var content = new StringContent(JsonConvert.SerializeObject(newInstrument), System.Text.Encoding.UTF8, "application/json");

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
