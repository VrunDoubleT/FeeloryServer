using FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.PostFeeds;

public class PostDeletedFeedConsumer : RabbitMqConsumerBase<PostDeletedMessage>
{
    public PostDeletedFeedConsumer(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory)
    {
    }

    protected override string QueueName => PostFeedQueues.FeedPostDeleted;
    protected override string RoutingKey => RoutingKeys.PostDeleted;

    protected override async Task ProcessAsync(IServiceScope scope, PostDeletedMessage message)
    {
        var service = scope.ServiceProvider.GetRequiredService<IPostFeedService>();
        await service.HandleDeletePostAsync(message.PostId);
    }
}