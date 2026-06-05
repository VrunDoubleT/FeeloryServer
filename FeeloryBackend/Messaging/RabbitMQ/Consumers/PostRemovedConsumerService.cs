using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostRemovedConsumerService : PostConsumerService
{
    public PostRemovedConsumerService(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory) { }

    protected override string QueueName => QueueNames.PostPermissionRemoved;
    protected override string RoutingKey => RoutingKeys.PostPermissionRemoved;
    protected override string Action => PostMessage.ActionRemoved;

    protected override async Task ProcessAsync(IServiceScope scope, PostMessage message)
    {
        var service = scope.ServiceProvider.GetRequiredService<IPostFeedService>();
        await service.HandleRemoveFeedsAsync(message);
    }
}