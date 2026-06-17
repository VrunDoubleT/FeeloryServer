using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;
using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

public class PostPublisher
{
    private readonly IEventBus _eventBus;

    public PostPublisher(
        IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <summary>
    /// Publish a post created message
    /// </summary>
    public async Task PublishPostCreatedAsync(PostCreatedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.PostCreated, message);
    }

    /// <summary>
    /// Publish a post permission removed message
    /// </summary>
    public async Task PublishPostUpdatedAsync(PostUpdatedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.PostUpdated, message);
    }

    /// <summary>
    /// Publish a post deleted message
    /// </summary>
    public async Task PublishPostDeletedAsync(PostDeletedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.PostDeleted, message);
    }
}