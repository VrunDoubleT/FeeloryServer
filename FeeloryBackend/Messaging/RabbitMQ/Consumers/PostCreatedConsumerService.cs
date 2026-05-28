using System.Text;
using System.Text.Json;
using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Constants;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Models.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ExchangeType = RabbitMQ.Client.ExchangeType;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostCreatedConsumerService : BackgroundService
{
    private readonly IRabbitMQConnectionFactory _factory;

    private readonly IServiceScopeFactory _scopeFactory;

    public PostCreatedConsumerService(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
    {
        _factory = factory;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _factory.CreateConnection();

        var channel = await connection.CreateChannelAsync(cancellationToken:stoppingToken);

        await channel.ExchangeDeclareAsync(
            RabbitMQConstants.MainExchange,
            ExchangeType.Topic,
            true,
            false,
            cancellationToken:
            stoppingToken
        );

        await channel.QueueDeclareAsync(
            QueueNames.PostCreated,
            true,
            false,
            false,
            cancellationToken:
            stoppingToken
        );

        await channel.QueueBindAsync(
            QueueNames.PostCreated,
            RabbitMQConstants.MainExchange,
            RoutingKeys.PostCreated,
            cancellationToken:
            stoppingToken
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());

                Console.WriteLine(json);
                
                var msg = JsonSerializer.Deserialize<PostCreatedMessage>(json);

                using var scope =_scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                if (msg?.AllowedUserIds!= null)
                {
                    var feeds = msg.AllowedUserIds
                        .Select(
                            x => new PostFeed
                            {
                                Id = Guid.NewGuid(),
                                ViewerId = x,
                                PostId = msg.PostId,
                                PostedAt = DateTime.UtcNow
                            });

                    await db.PostFeeds.AddRangeAsync(feeds);

                    await db.SaveChangesAsync();
                }

                await channel.BasicAckAsync(
                        args.DeliveryTag,
                        false,
                        stoppingToken
                    );
            }
            catch
            {
                await channel.BasicNackAsync(
                        args.DeliveryTag,
                        false,
                        true,
                        stoppingToken
                    );
            }
        };

        await channel.BasicConsumeAsync(
                QueueNames.PostCreated,
                false,
                consumer,
                cancellationToken:
                stoppingToken
            );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}