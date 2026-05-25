using System.Text;
using System.Text.Json;
using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Constants;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.BackgroundServices;

public class DayShareFeedService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    public DayShareFeedService(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _configuration = configuration;
        _scopeFactory  = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"],
            Port     = int.Parse(_configuration["RabbitMQ:Port"]!),
            UserName = _configuration["RabbitMQ:Username"],
            Password = _configuration["RabbitMQ:Password"]
        };

        var connection = await factory.CreateConnectionAsync(stoppingToken);
        var channel    = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMQConstants.MainExchange,
            type:     ExchangeType.Direct,
            durable:  true,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue:      QueueNames.DayShareFeed,
            durable:    true,
            exclusive:  false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue:      QueueNames.DayShareFeed,
            exchange:   RabbitMQConstants.MainExchange,
            routingKey: QueueNames.DayShareFeed,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json    = Encoding.UTF8.GetString(ea.Body.Span);
                var message = JsonSerializer.Deserialize<DayShareFeedMessage>(json);

                if (message is null || message.Action != "CREATED")
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                foreach (var viewerId in message.ViewerIds.Distinct())
                {
                    bool alreadyExists = await db.DayShareFeeds.AnyAsync(
                        f => f.DayShareId == message.DayShareId && f.ViewerId == viewerId,
                        stoppingToken);

                    if (alreadyExists)
                        continue;

                    db.DayShareFeeds.Add(new DayShareFeed
                    {
                        Id         = Guid.NewGuid(),
                        DayShareId = message.DayShareId,
                        ViewerId   = viewerId,
                        PostedAt   = DateTime.UtcNow
                    });
                }

                await db.SaveChangesAsync(stoppingToken);

                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue:     QueueNames.DayShareFeed,
            autoAck:   false,
            consumer:  consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}