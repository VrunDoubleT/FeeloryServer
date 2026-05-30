using System.Text;
using System.Text.Json;
using FeeloryBackend.Messaging.RabbitMQ.Constants;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class ReactionTaskConsumer : BackgroundService
{
    private readonly IRabbitMQConnectionFactory _factory;

    public ReactionTaskConsumer(
        IRabbitMQConnectionFactory factory)
    {
        _factory = factory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connection = await _factory.CreateConnection();

        var channel = await connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMQConstants.MainExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: QueueNames.TaskReactionAdded,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: QueueNames.TaskReactionAdded,
            exchange: RabbitMQConstants.MainExchange,
            routingKey: RoutingKeys.TaskReactionAdded,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(
                    args.Body.ToArray());

                var message = JsonSerializer.Deserialize<TaskReactionMessage>(json);

                if (message is not null)
                {
                    // TODO:
                    // check task completion here
                }

                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    false,
                    stoppingToken);
            }
            catch
            {
                await channel.BasicNackAsync(
                    args.DeliveryTag,
                    false,
                    true,
                    stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueNames.TaskReactionAdded,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }
}