using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using FeeloryBackend.Messaging.RabbitMQ.Constants;

namespace FeeloryBackend.Messaging.RabbitMQ;

public interface IEventBus
{
    Task PublishAsync<T>(string routingKey, T message);
}

public class RabbitMQEventBus : IEventBus
{
    private readonly IRabbitMQConnectionFactory _factory;

    public RabbitMQEventBus(IRabbitMQConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task PublishAsync<T>(string routingKey, T message)
    {
        await using var connection = await _factory.CreateConnection();
        await using var channel = await connection.CreateChannelAsync();

        // Declare exchange (topic)
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMQConstants.MainExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(
            exchange: RabbitMQConstants.MainExchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body
        );
    }
}