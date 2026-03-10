namespace TiendaMusica.Tests.Infrastructure.Entrypoint.Rest
{
    public class InstrumentController_ITest: IClassFixture<WebAppTestFactory>
    {
        private readonly HttpClient _client;

        public InstrumentController_ITest()
        {
            var factory = new WebAppTestFactory();
            _client = factory.CreateClient();
        }

        [Fact]
        public void Test1()
        {
            // Arrange
            var url = "/v1/instrument";

            // Act
            var response = _client.GetAsync(url);

            // Assert
            Assert.NotNull(response);
        }
    }
}
