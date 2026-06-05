using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

public class PostPublisher
{
    private readonly IEventBus _eventBus;

    public PostPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task PublishPostAsync(PostMessage message)
    {
        var routingKey = message.Action switch
        {
            PostMessage.ActionCreated => RoutingKeys.PostCreated,
            PostMessage.ActionAdded   => RoutingKeys.PostPermissionAdded,
            PostMessage.ActionRemoved => RoutingKeys.PostPermissionRemoved,
            PostMessage.ActionDeleted => RoutingKeys.PostDeleted,
            _ => throw new ArgumentException($"Unknown action: {message.Action}")
        };
        await _eventBus.PublishAsync(routingKey, message);
    }
}