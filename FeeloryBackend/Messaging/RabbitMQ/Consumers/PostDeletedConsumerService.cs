using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Messaging.RabbitMQ.Routing;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers;

public class PostDeletedConsumerService : RabbitMqConsumerBase<PostMessage>
{
    public PostDeletedConsumerService(IRabbitMQConnectionFactory factory, IServiceScopeFactory scopeFactory)
        : base(factory, scopeFactory) { }

    protected override string QueueName => QueueNames.PostDeleted;
    protected override string RoutingKey => RoutingKeys.PostDeleted;

    protected override async Task ProcessAsync(IServiceScope scope, PostMessage message)
    {
        var service = scope.ServiceProvider.GetRequiredService<IPostFeedService>();
        await service.HandleDeletePostAsync(message);
    }
}