using FeeloryBackend.Messaging.RabbitMQ.Messages.DayShares;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.DayShareFeeds;

public class DayShareCreatedFeedConsumer : RabbitMqConsumerBase<DayShareCreatedMessage>
{
    public DayShareCreatedFeedConsumer(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => DayShareFeedQueues.FeedPostCreated;
    protected override string RoutingKey => RoutingKeys.DayShareCreated;

    protected override async Task ProcessAsync(IServiceScope scope, DayShareCreatedMessage message)
    {
        var service = scope.ServiceProvider.GetRequiredService<IDayShareFeedService>();
        await service.HandleAddFeedsAsync(message.DayShareId, message.RecipientIds);
    }
}