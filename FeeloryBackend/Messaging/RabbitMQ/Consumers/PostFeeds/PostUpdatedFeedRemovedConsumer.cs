using FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.PostFeeds;

public class PostUpdatedFeedRemovedConsumer : RabbitMqConsumerBase<PostUpdatedMessage>
{
    public PostUpdatedFeedRemovedConsumer(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => PostFeedQueues.FeedPostUpdatedRemoved;
    protected override string RoutingKey => RoutingKeys.PostUpdated;

    protected override async Task ProcessAsync(IServiceScope scope, PostUpdatedMessage message)
    {
        var service = scope.ServiceProvider.GetRequiredService<IPostFeedService>();
        await service.HandleRemoveFeedsAsync(message.PostId, message.RemovedViewerIds);
    }
}