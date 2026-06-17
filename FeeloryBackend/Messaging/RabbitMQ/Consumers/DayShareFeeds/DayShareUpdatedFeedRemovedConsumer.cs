using FeeloryBackend.Messaging.RabbitMQ.Messages.DayShares;
using FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.DayShareFeeds;

public class DayShareUpdatedFeedRemovedConsumer : RabbitMqConsumerBase<DayShareUpdatedMessage>
{
    public DayShareUpdatedFeedRemovedConsumer(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => DayShareFeedQueues.FeedPostUpdatedRemoved;
    protected override string RoutingKey => RoutingKeys.DayShareUpdated;

    protected override async Task ProcessAsync(IServiceScope scope, DayShareUpdatedMessage message)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Day share updated feed removed <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
        var service = scope.ServiceProvider.GetRequiredService<IDayShareFeedService>();
        await service.HandleRemovedAsync(message.DayShareId, message.RemovedViewerIds);
    }
}