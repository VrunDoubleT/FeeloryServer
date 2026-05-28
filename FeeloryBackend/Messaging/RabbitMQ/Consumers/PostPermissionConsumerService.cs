using System.Text;
using System.Text.Json;
using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Constants;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ExchangeType = RabbitMQ.Client.ExchangeType;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostPermissionConsumerService : BackgroundService
{
    private readonly IRabbitMQConnectionFactory _factory;
    private readonly IServiceScopeFactory _scopeFactory;

    public PostPermissionConsumerService(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
    {
        _factory = factory;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _factory.CreateConnection();

        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Exchange
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMQConstants.MainExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        // Queue
        await channel.QueueDeclareAsync(
            queue: QueueNames.PostPermission,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        // Bind
        await channel.QueueBindAsync(
            queue: QueueNames.PostPermission,
            exchange: RabbitMQConstants.MainExchange,
            routingKey: RoutingKeys.PostPermissionChanged,
            cancellationToken: stoppingToken
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<PostPermissionChangedMessage>(json);
                if (message == null)
                {
                    await channel.BasicAckAsync(args.DeliveryTag, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // REMOVE
                if (message.RemovedUserIds.Any())
                {
                    var removeFeeds = await db.PostFeeds
                        .Where(x => x.PostId == message.PostId && message.RemovedUserIds.Contains(x.ViewerId))
                        .ToListAsync();
                    db.PostFeeds.RemoveRange(removeFeeds);
                }

                // ADD
                if (message.AddedUserIds.Any())
                {
                    var feeds = message.AddedUserIds
                        .Select(userId => new PostFeed
                            {
                                Id = Guid.NewGuid(),
                                PostId = message.PostId,
                                ViewerId = userId,
                                PostedAt = DateTime.UtcNow
                            }
                        );
                    await db.PostFeeds.AddRangeAsync(feeds);
                }

                await db.SaveChangesAsync();

                await channel.BasicAckAsync(args.DeliveryTag, false);
            }
            catch {
                await channel.BasicNackAsync(args.DeliveryTag, false, true);
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueNames.PostPermission,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}