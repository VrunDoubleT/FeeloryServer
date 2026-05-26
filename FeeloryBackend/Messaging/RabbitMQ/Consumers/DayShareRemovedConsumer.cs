using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Implementations;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class DayShareRemovedConsumer : DayShareFeedConsumerService
{
    public DayShareRemovedConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory) { }

    protected override string QueueName  => QueueNames.DayShareRemoved;
    protected override string RoutingKey => RoutingKeys.DayShareRemoved;
    protected override string Action     => DayShareFeedMessage.ActionRemoved;

    protected override Task ProcessAsync(AppDbContext db, DayShareFeedMessage message)
        => DayShareFeedService.HandleRemovedAsync(db, message);
}