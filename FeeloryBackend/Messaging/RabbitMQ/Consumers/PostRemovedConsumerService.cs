using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostRemovedConsumerService : PostConsumerService
{
    private readonly IPostFeedService  _postFeedService;

    public PostRemovedConsumerService(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory,
        IPostFeedService postFeedService)
        : base(factory, scopeFactory)
    {
        _postFeedService = postFeedService;
    }

    protected override string QueueName => QueueNames.PostPermissionRemoved;
    protected override string RoutingKey => RoutingKeys.PostPermissionRemoved;
    protected override string Action     => PostMessage.ActionRemoved;

    protected override Task ProcessAsync(AppDbContext db, PostMessage message)
        => _postFeedService.HandleRemoveFeedsAsync(db, message);
}