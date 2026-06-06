using FeeloryBackend.Messaging.RabbitMQ.Messages.DayShares;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.DayShareFeeds;

public class DayShareDeletedFeedConsumer : RabbitMqConsumerBase<DayShareDeletedMessage>
{
    public DayShareDeletedFeedConsumer(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => DayShareFeedQueues.FeedPostDeleted;
    protected override string RoutingKey => RoutingKeys.DayShareDeleted;

    protected override async Task ProcessAsync(IServiceScope scope, DayShareDeletedMessage message)
    {
        var service = scope.ServiceProvider.GetRequiredService<IDayShareFeedService>();
        await service.HandleDeletedAsync(message.DayShareId);
    }
}