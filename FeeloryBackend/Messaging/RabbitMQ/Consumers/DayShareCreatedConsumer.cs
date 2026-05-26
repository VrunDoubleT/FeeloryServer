using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Implementations;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class DayShareCreatedConsumer : DayShareFeedConsumerService
{
    public DayShareCreatedConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory) { }

    protected override string QueueName  => QueueNames.DayShareCreated;
    protected override string RoutingKey => RoutingKeys.DayShareCreated;
    protected override string Action     => DayShareFeedMessage.ActionCreated;

    protected override Task ProcessAsync(AppDbContext db, DayShareFeedMessage message)
        => DayShareFeedService.HandleAddFeedsAsync(db, message);
}