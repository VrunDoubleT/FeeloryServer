using System.Text;
using System.Text.Json;
using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Constants;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ExchangeType = RabbitMQ.Client.ExchangeType;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostDeletedConsumerService : BackgroundService
{
    private readonly IRabbitMQConnectionFactory _factory;
    private readonly IServiceScopeFactory _scopeFactory;

    public PostDeletedConsumerService(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
    {
        _factory = factory;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _factory.CreateConnection();

        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            RabbitMQConstants.MainExchange,
            ExchangeType.Topic,
            true,
            false
        );

        await channel.QueueDeclareAsync(
            QueueNames.PostDeleted,
            true,
            false,
            false
        );

        await channel.QueueBindAsync(
            QueueNames.PostDeleted,
            RabbitMQConstants.MainExchange,
            RoutingKeys.PostDeleted
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonSerializer.Deserialize<PostDeletedMessage>(json);
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var feeds = await db.PostFeeds.Where(x => x.PostId == message!.PostId).ToListAsync();
            db.PostFeeds.RemoveRange(feeds);
            await db.SaveChangesAsync();
            await channel.BasicAckAsync(args.DeliveryTag, false);
        };

        await channel.BasicConsumeAsync(QueueNames.PostDeleted, false, consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}