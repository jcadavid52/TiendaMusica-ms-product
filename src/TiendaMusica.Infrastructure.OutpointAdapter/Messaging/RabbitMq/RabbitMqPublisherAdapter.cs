using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Messaging.RabbitMq
{
    public class RabbitMqPublisherAdapter : IMessagePublisherPort
    {
        private readonly IConnection _connection;
        private readonly string _exchange;

        public RabbitMqPublisherAdapter(
            IConnection connection,
            IConfiguration configuration
            )
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

            _exchange = configuration.GetSection("RabbitMQ:Exchange").Value
            ?? "tienda.musica.events";

        }
        public async Task PublishAsync<T>(T @event) where T : class
        {
            using var channel = await _connection.CreateChannelAsync();
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            await channel.ExchangeDeclareAsync(
            exchange: _exchange,
            type: ExchangeType.Topic,
            durable: true);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: typeof(T).Name,
            mandatory: true,
            basicProperties: properties,
            body: body);
        }
    }
}
