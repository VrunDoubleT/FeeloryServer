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
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class DayShareFeedConsumerService : BackgroundService
{
    private readonly IRabbitMQConnectionFactory _factory;
    private readonly IServiceScopeFactory _scopeFactory;

    public DayShareFeedConsumerService(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
    {
        _factory = factory;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connection =
            await _factory.CreateConnection();

        var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMQConstants.MainExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: QueueNames.DayShareFeed,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: QueueNames.DayShareFeed,
            exchange: RabbitMQConstants.MainExchange,
            routingKey: RoutingKeys.DayShareFeed,
            cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var json =
                    Encoding.UTF8.GetString(
                        args.Body.ToArray());

                var message =
                    JsonSerializer.Deserialize<
                        DayShareFeedMessage>(json);

                if (message is null)
                    return;

                using var scope =
                    _scopeFactory.CreateScope();

                var db =
                    scope.ServiceProvider
                        .GetRequiredService<AppDbContext>();

                switch (message.Action)
                {
                    case "CREATED":
                        await HandleCreatedAsync(
                            db,
                            message);
                        break;

                    case "UPDATED":
                        await HandleUpdatedAsync(
                            db,
                            message);
                        break;

                    case "DELETED":
                        await HandleDeletedAsync(
                            db,
                            message);
                        break;
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
            queue: QueueNames.DayShareFeed,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    private static async Task HandleCreatedAsync(
        AppDbContext db,
        DayShareFeedMessage message)
    {
        foreach (var viewerId in message.ViewerIds)
        {
            bool exists =
                await db.DayShareFeeds.AnyAsync(x =>
                    x.DayShareId == message.DayShareId
                    &&
                    x.ViewerId == viewerId);

            if (exists)
                continue;

            db.DayShareFeeds.Add(
                new DayShareFeed
                {
                    Id = Guid.NewGuid(),
                    DayShareId = message.DayShareId,
                    ViewerId = viewerId,
                    PostedAt = DateTime.UtcNow
                });
        }

        await db.SaveChangesAsync();
    }

    private static async Task HandleUpdatedAsync(
        AppDbContext db,
        DayShareFeedMessage message)
    {
        var oldFeeds =
            await db.DayShareFeeds
                .Where(x =>
                    x.DayShareId ==
                    message.DayShareId)
                .ToListAsync();

        var oldViewerIds =
            oldFeeds
                .Select(x => x.ViewerId)
                .ToHashSet();

        var newViewerIds =
            message.ViewerIds.ToHashSet();

        var removedViewerIds =
            oldViewerIds.Except(newViewerIds);

        var addedViewerIds =
            newViewerIds.Except(oldViewerIds);

        db.DayShareFeeds.RemoveRange(
            oldFeeds.Where(x =>
                removedViewerIds.Contains(
                    x.ViewerId)));

        foreach (var viewerId in addedViewerIds)
        {
            db.DayShareFeeds.Add(
                new DayShareFeed
                {
                    Id = Guid.NewGuid(),
                    DayShareId = message.DayShareId,
                    ViewerId = viewerId,
                    PostedAt = DateTime.UtcNow
                });
        }

        await db.SaveChangesAsync();
    }

    private static async Task HandleDeletedAsync(
        AppDbContext db,
        DayShareFeedMessage message)
    {
        var feeds =
            await db.DayShareFeeds
                .Where(x =>
                    x.DayShareId ==
                    message.DayShareId)
                .ToListAsync();

        db.DayShareFeeds.RemoveRange(feeds);

        await db.SaveChangesAsync();
    }
}