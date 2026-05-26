using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Implementations;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class DayShareDeletedConsumer : DayShareFeedConsumerService
{
    public DayShareDeletedConsumer(
        IRabbitMQConnectionFactory factory,
        IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory) { }

    protected override string QueueName  => QueueNames.DayShareDeleted;
    protected override string RoutingKey => RoutingKeys.DayShareDeleted;
    protected override string Action     => DayShareFeedMessage.ActionDeleted;

    protected override Task ProcessAsync(AppDbContext db, DayShareFeedMessage message)
        => DayShareFeedService.HandleDeletedAsync(db, message);
}