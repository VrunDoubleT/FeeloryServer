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

    // CREATE POST
    public async Task PublishPostAsync(PostCreatedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.PostCreated, message);
    }
    
    // UPDATE PERMISSION
    public async Task PublishPermissionChangedAsync(PostPermissionChangedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.PostPermissionChanged, message);
    }
    
    // DELETE POST
    public async Task PublishPostDeletedAsync(PostDeletedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.PostDeleted, message);
    }
}