using FeeloryBackend.Messaging.RabbitMQ.Messages.Reactions;
using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

public class PostReactionPublisher
{
    private readonly IEventBus _eventBus;

    public PostReactionPublisher(
        IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <summary>
    /// Publish a post reaction added message
    /// </summary>
    public async Task PublishPostReactionAddedAsync(PostReactionAddedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.PostReactionAdded, message);
    }
}