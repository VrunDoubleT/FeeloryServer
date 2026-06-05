using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostAddedConsumerService : RabbitMqConsumerBase<PostMessage>
{
    public PostAddedConsumerService(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory) { }

    protected override string QueueName => QueueNames.PostPermissionAdded;
    protected override string RoutingKey => RoutingKeys.PostPermissionAdded;

    protected override async Task ProcessAsync(IServiceScope scope, PostMessage message)
    {
        var service = scope.ServiceProvider.GetRequiredService<IPostFeedService>();
        await service.HandleAddFeedsAsync(message);
    }
}