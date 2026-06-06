using FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.PostFeeds;

public class PostCreatedFeedConsumer : RabbitMqConsumerBase<PostCreatedMessage>
{
    public PostCreatedFeedConsumer(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => PostFeedQueues.FeedPostCreated;
    protected override string RoutingKey => RoutingKeys.PostCreated;

    protected override async Task ProcessAsync(IServiceScope scope, PostCreatedMessage message)
    {
        var service = scope.ServiceProvider.GetRequiredService<IPostFeedService>();
        await service.HandleAddFeedsAsync(message.PostId, message.RecipientIds);
    }
}