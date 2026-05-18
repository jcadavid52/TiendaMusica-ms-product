using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TiendaMusica.Domain.Models.Result;
using TiendaMusica.Domain.Ports;

namespace TiendaMusica.Infrastructure.OutpointAdapter.Messaging.RabbitMq
{
    public class RabbitMqPublisherAdapter : IMessagePublisherPort
    {
        private readonly IConnection _connection;
        private readonly string _exchange;
        private static bool _exchangeDeclared = false;

        public RabbitMqPublisherAdapter(
            IConnection connection,
            IConfiguration configuration
            )
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));

            _exchange = configuration.GetSection("RabbitMQ:Exchange").Value
            ?? "tienda.musica.events";

        }

        private async Task EnsureExchangeExists(IChannel channel)
        {
            if (!_exchangeDeclared)
            {
                await channel.ExchangeDeclareAsync(
                    exchange: _exchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _exchangeDeclared = true;
            }
        }

        public async Task<Results<bool>> PublishAsync<T>(T @event) where T : class
        {
            var results = new Results<bool>();

            try
            {
                using var channel = await _connection.CreateChannelAsync();
                await EnsureExchangeExists(channel);
                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json",
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                };

                await channel.BasicPublishAsync(
                exchange: _exchange,
                routingKey: @event.GetType().Name,
                mandatory: true,
                basicProperties: properties,
                body: body);

                results.Result = true;
            }
            catch (Exception ex)
            {
                results.Result = false;
                return results.AddError(ErrorCode.PUBLISH_MESSAGE_ERROR, $"Failed to publish message: {ex.Message}");
            }

            return results;
        }
    }
}
