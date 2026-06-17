using FeeloryBackend.Messaging.RabbitMQ.Messages.DayShares;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.DayShareFeeds;

public class DayShareUpdatedFeedAddedConsumer : RabbitMqConsumerBase<DayShareUpdatedMessage>
{
    public DayShareUpdatedFeedAddedConsumer(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => DayShareFeedQueues.FeedPostUpdatedAdded;
    protected override string RoutingKey => RoutingKeys.DayShareUpdated;

    protected override async Task ProcessAsync(IServiceScope scope, DayShareUpdatedMessage message)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Day share updated feed addded <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
        var service = scope.ServiceProvider.GetRequiredService<IDayShareFeedService>();
        await service.HandleAddFeedsAsync(message.DayShareId, message.AddedViewerIds);
    }
}