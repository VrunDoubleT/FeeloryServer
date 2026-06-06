using FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.PostFeeds;

public class PostUpdatedFeedAddedConsumer : RabbitMqConsumerBase<PostUpdatedMessage>
{
    public PostUpdatedFeedAddedConsumer(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => PostFeedQueues.FeedPostUpdatedAdded;
    protected override string RoutingKey => RoutingKeys.PostUpdated;

    protected override async Task ProcessAsync(IServiceScope scope, PostUpdatedMessage message)
    {
        var service = scope.ServiceProvider.GetRequiredService<IPostFeedService>();
        await service.HandleAddFeedsAsync(message.PostId, message.AddedViewerIds);
    }
}