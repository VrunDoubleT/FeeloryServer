using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Implementations;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class DayShareAddedConsumer : DayShareFeedConsumerService
{
    public DayShareAddedConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory) { }

    protected override string QueueName  => QueueNames.DayShareAdded;
    protected override string RoutingKey => RoutingKeys.DayShareAdded;
    protected override string Action     => DayShareFeedMessage.ActionAdded;

    protected override Task ProcessAsync(AppDbContext db, DayShareFeedMessage message)
        => DayShareFeedService.HandleAddFeedsAsync(db, message);
}