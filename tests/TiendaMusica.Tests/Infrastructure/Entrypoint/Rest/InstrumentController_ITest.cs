using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Collections;
using System.Text;
using TiendaMusica.Application.Dtos;
using TiendaMusica.Application.UseCases.Instruments;
using TiendaMusica.Domain.Dtos;
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
            _client = factory.Client ?? throw new ArgumentNullException(nameof(factory.Client));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsSortDirectionAsc_WhenUseCaseReturnsDataStatusCode200()
        {
            //Arrange
            await _factory.SeedInstrumentDatabaseAsync();
            string url = "/v1/instrument?sortDirection=Asc&orderBy=creationdateutc";

            //Act
            var response = await _client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseString);
            var instrumentsInDb = await _factory.GetAllInstrumentDatabase();

            //Assert
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal(instrumentsInDb.Count, responseObject.Result.Count);
            Assert.Equal(instrumentsInDb[0].Name, responseObject.Result[9].Name);
            Assert.Equal(instrumentsInDb[1].Name, responseObject.Result[8].Name);
            Assert.Equal(instrumentsInDb[2].Name, responseObject.Result[7].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsSortDirectionDesc_WhenUseCaseReturnsDataStatusCode200()
        {
            //Arrange
            await _factory.SeedInstrumentDatabaseAsync();
            string url = "/v1/instrument?sortDirection=Desc&orderBy=creationdateutc";

            //Act
            var response = await _client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseString);
            var instrumentsInDb = await _factory.GetAllInstrumentDatabase();
            //Assert
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal(instrumentsInDb.Count, responseObject.Result.Count);
            Assert.Equal(instrumentsInDb[9].Name, responseObject.Result[9].Name);
            Assert.Equal(instrumentsInDb[8].Name, responseObject.Result[8].Name);
            Assert.Equal(instrumentsInDb[7].Name, responseObject.Result[7].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsFilteredBySearch_WhenSearchTermMatches()
        {
            //Arrange
            string searchTerm = _factory.InitialInstruments[0].Name;
            string url = $"/v1/instrument?search={searchTerm}";

            //Act
            var response = await _client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseString);

            //Assert
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Single(responseObject.Result);
            Assert.Equal(_factory.InitialInstruments[0].Name, responseObject.Result[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnInstrumentsPaginated_WhenPageParametersAreProvided()
        {
            //Arrange
            string urlPage1 = "/v1/instrument?pageNumber=1&pageSize=5&sortDirection=Desc&orderBy=creationdateutc";
            await _factory.SeedInstrumentDatabaseAsync();
            //Act - Page 1 (most recent first with Desc)
            var responsePage1 = await _client.GetAsync(urlPage1);
            var responseStringPage1 = await responsePage1.Content.ReadAsStringAsync();
            var responseObjectPage1 = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseStringPage1);
            var instrumentsInDb = await _factory.GetAllInstrumentDatabase();

            //Assert - Page 1
            Assert.NotNull(responseObjectPage1);
            Assert.NotNull(responseObjectPage1.Result);
            Assert.True(responseObjectPage1.IsSuccess);
            Assert.Equal(5, responseObjectPage1.Result.Count);
            Assert.Equal(instrumentsInDb[0].Name, responseObjectPage1.Result[0].Name);
            Assert.Equal(instrumentsInDb[4].Name, responseObjectPage1.Result[4].Name);
            //Act - Page 2
            string urlPage2 = "/v1/instrument?pageNumber=2&pageSize=5&sortDirection=Desc&orderBy=creationdateutc";
            var responsePage2 = await _client.GetAsync(urlPage2);
            var responseStringPage2 = await responsePage2.Content.ReadAsStringAsync();
            var responseObjectPage2 = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(responseStringPage2);

            //Assert - Page 2
            Assert.NotNull(responseObjectPage2);
            Assert.NotNull(responseObjectPage2.Result);
            Assert.True(responseObjectPage2.IsSuccess);
            Assert.Equal(5, responseObjectPage2.Result.Count);
            Assert.Equal(instrumentsInDb[5].Name, responseObjectPage2.Result[0].Name);
            Assert.Equal(instrumentsInDb[9].Name, responseObjectPage2.Result[4].Name);
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
            var errorClient = CreateErrorClientWithMockedUseCase(mock =>
            {
                mock.Setup(m => m.GetAllAsync(It.IsAny<InstrumentGetAllQueryParametersDto>()))
                    .ReturnsAsync(new Results<IList<Instrument>>
                    {
                        Errors = new List<TiendaMusicaError>
                        {
                            new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                        },
                    });
            });

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

            var newInstrument = new InstrumentCreateRequest("Batería Eléctrica",
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
        public async Task CreateAsync_WhenRequestContaintsError_ShouldReturnFailureResultStatus400(InstrumentCreateRequest request, Results<InstrumentResponse> expected)
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
            var newInstrument = new InstrumentCreateRequest("Guitarra Eléctrica",
                "Descripción test",
                InstrumentType.Stringed,
                1500.00m,
                1
            );
            var content = new StringContent(JsonConvert.SerializeObject(newInstrument), Encoding.UTF8, "application/json");

            // Act
            var errorClient = CreateErrorClientWithMockedUseCase(mock =>
            {
                mock.Setup(m => m.CreateAsync(It.IsAny<InstrumentCreateCommand>()))
                    .ReturnsAsync(new Results<Instrument>
                    {
                        Errors = new List<TiendaMusicaError>
                        {
                            new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                        },
                    });
            });

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
            var url = $"/v1/instrument/{_factory.InitialInstruments[0].Id}";
            int codeExpected = 200;

            // Act
            var response = await _client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<InstrumentResponse>>(responseString);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal(_factory.InitialInstruments[0].Name, responseObject.Result.Name);
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

            var errorClient = CreateErrorClientWithMockedUseCase(mock =>
            {
                mock.Setup(m => m.GetByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Results<Instrument>
                    {
                        Errors = new List<TiendaMusicaError>
                        {
                            new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                        },
                    });
            });

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

        [Fact]
        public async Task DeleteMultipleAsync_WhenInstrumentsExist_ShouldReturnSuccessResultStatus200()
        {
            // Arrange
            var idsToDelete = new List<string>
            {
                _factory.InitialInstruments[9].Id,
                _factory.InitialInstruments[8].Id,
                _factory.InitialInstruments[7].Id
            };

            var deleteUrl = "/v1/instrument/delete-multiple";
            var deleteRequest = new InstrumentDeleteMultipleRequest(idsToDelete);
            var content = new StringContent(JsonConvert.SerializeObject(deleteRequest), Encoding.UTF8, "application/json");
            int codeExpected = 200;

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl)
            {
                Content = content
            };
            var response = await _client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<int>>(responseString);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal(3, responseObject.Result);
            Assert.Equal(codeExpected, (int)response.StatusCode);

            // Verify deletion
            var verifyUrl = "/v1/instrument";
            var verifyResponse = await _client.GetAsync(verifyUrl);
            var verifyString = await verifyResponse.Content.ReadAsStringAsync();
            var verifyObject = JsonConvert.DeserializeObject<Results<IList<InstrumentResponse>>>(verifyString);
            Assert.Equal(7, verifyObject?.Result?.Count);
        }

        [Fact]
        public async Task DeleteMultipleAsync_WhenEmptyIdList_ShouldReturnFailureResultStatus400()
        {
            // Arrange
            var deleteUrl = "/v1/instrument/delete-multiple";
            var deleteRequest = new InstrumentDeleteMultipleRequest(new List<string>());
            var content = new StringContent(JsonConvert.SerializeObject(deleteRequest), Encoding.UTF8, "application/json");
            int codeExpected = 400;

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl)
            {
                Content = content
            };
            var response = await _client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<int>>(responseString);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.False(responseObject.IsSuccess);
            Assert.True(responseObject.Errors.Any());
            Assert.Equal(ErrorCode.VALIDATION_ERROR, responseObject.Errors[0].ErrorCode);
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Fact]
        public async Task DeleteMultipleAsync_WhenUseCaseReturnsErrors_ShouldReturnFailureResultStatus500()
        {
            // Arrange
            var deleteUrl = "/v1/instrument/delete-multiple";
            var deleteRequest = new InstrumentDeleteMultipleRequest(new List<string> { "id1", "id2" });
            var content = new StringContent(JsonConvert.SerializeObject(deleteRequest), Encoding.UTF8, "application/json");
            int codeExpected = 500;

            var errorClient = CreateErrorClientWithMockedUseCase(mock =>
            {
                mock.Setup(m => m.DeleteMultipleAsync(It.IsAny<InstrumentDeleteMultipleCommand>()))
                    .ReturnsAsync(new Results<int>
                    {
                        Errors = new List<TiendaMusicaError>
                        {
                            new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                        },
                    });
            });

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl)
            {
                Content = content
            };
            var response = await errorClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<int>>(responseString);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.False(responseObject.IsSuccess);
            Assert.True(responseObject.Errors.Any());
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_WhenInstrumentExists_ShouldReturnSuccessResultStatus200()
        {
            // Arrange
            var instruments = await _factory.GetAllInstrumentDatabase();
            var instrumentToUpdate = instruments[0];

            var updateRequest = new InstrumentUpdateRequest(
                instrumentToUpdate.Id,
                "Batería test Actualizada",
                "Descripción tests actualizada",
                InstrumentType.Percussion
            );

            var url = $"/v1/instrument/{instrumentToUpdate.Id}";
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
            int codeExpected = 200;

            // Act
            var response = await _client.PutAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<InstrumentResponse>>(responseString);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.NotNull(responseObject.Result);
            Assert.True(responseObject.IsSuccess);
            Assert.Equal(updateRequest.Name, responseObject.Result.Name);
            Assert.Equal(updateRequest.Description, responseObject.Result.Description);
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_WhenIdMismatch_ShouldReturnFailureResultStatus400()
        {
            // Arrange
            var instrumentToUpdate = _factory.InitialInstruments[0];

            var updateRequest = new InstrumentUpdateRequest(
                "different-id",
                "Guitarra Acústica Actualizada",
                "Descripción actualizada",
                InstrumentType.Stringed
            );

            var url = $"/v1/instrument/{instrumentToUpdate.Id}";
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
            int codeExpected = 400;

            // Act
            var response = await _client.PutAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<InstrumentResponse>>(responseString);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.False(responseObject.IsSuccess);
            Assert.True(responseObject.Errors.Any());
            Assert.Contains("ID en la ruta", responseObject.Errors[0].Message);
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Theory]
        [ClassData(typeof(UpdateValidationTestData))]
        public async Task UpdateAsync_WhenRequestContainsErrors_ShouldReturnFailureResultStatus400(InstrumentUpdateRequest request, Results<InstrumentResponse> expected)
        {
            // Arrange
            var url = $"/v1/instrument/{request.Id}";
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            int codeExpected = 400;

            // Act
            var response = await _client.PutAsync(url, content);
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
        public async Task UpdateAsync_WhenInstrumentNotExists_ShouldReturnFailureResultStatus404()
        {
            // Arrange
            var updateRequest = new InstrumentUpdateRequest(
                "non-existent-id",
                "Guitarra Acústica Actualizada",
                "Descripción actualizada",
                InstrumentType.Stringed
            );

            var url = "/v1/instrument/non-existent-id";
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
            int codeExpected = 404;

            // Act
            var response = await _client.PutAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<Results<InstrumentResponse>>(responseString);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(responseObject);
            Assert.False(responseObject.IsSuccess);
            Assert.True(responseObject.Errors.Any());
            Assert.Equal(codeExpected, (int)response.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_WhenUseCaseReturnsErrors_ShouldReturnFailureResultStatus500()
        {
            // Arrange
            var instrumentToUpdate = _factory.InitialInstruments[0];

            var updateRequest = new InstrumentUpdateRequest(
                instrumentToUpdate.Id,
                "Guitarra Acústica Actualizada",
                "Descripción actualizada",
                InstrumentType.Stringed
            );

            var url = $"/v1/instrument/{instrumentToUpdate.Id}";
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
            int codeExpected = 500;

            var errorClient = CreateErrorClientWithMockedUseCase(mock =>
            {
                mock.Setup(m => m.UpdateAsync(It.IsAny<InstrumentUpdateCommand>()))
                    .ReturnsAsync(new Results<Instrument>
                    {
                        Errors = new List<TiendaMusicaError>
                        {
                            new TiendaMusicaError(ErrorCode.SERVER_ERROR, "Error en el servidor")
                        },
                    });
            });

            // Act
            var response = await errorClient.PutAsync(url, content);
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

        private HttpClient CreateErrorClientWithMockedUseCase(Action<Mock<IInstrumentUseCase>> setupMock)
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IInstrumentUseCase));
                    if (descriptor != null) services.Remove(descriptor);

                    var mockInstrumentUsecase = new Mock<IInstrumentUseCase>();
                    setupMock(mockInstrumentUsecase);

                    services.AddScoped(_ => mockInstrumentUsecase.Object);
                });
            }).CreateClient();
        }

        private class ValidationTestData : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                 new object[]
                 {
                    new InstrumentCreateRequest("","Descripción test",InstrumentType.Stringed,1500.00m,1),
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
                    new InstrumentCreateRequest("Nombre instrumento","",InstrumentType.Stringed,1500.00m,1),
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
                    new InstrumentCreateRequest("Nombre instrumento","Descripción test",InstrumentType.Stringed,0,1),
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
                    new InstrumentCreateRequest("Nombre instrumento","Descripción test",InstrumentType.Stringed,900m,-1),
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

        private class UpdateValidationTestData : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                 new object[]
                 {
                    new InstrumentUpdateRequest("test-id", "", "Descripción test", InstrumentType.Stringed),
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
                    new InstrumentUpdateRequest("test-id", "Nombre instrumento", "", InstrumentType.Stringed),
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
