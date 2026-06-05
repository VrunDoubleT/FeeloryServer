using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

public class ReactionPublisher
{
    private readonly IEventBus _eventBus;

    public ReactionPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task PublishNotificationAsync(
        ReactionMessage message)
    {
        await _eventBus.PublishAsync(
            RoutingKeys.Reaction,
            message);
    }

    public async Task PublishTaskAsync(
        TaskReactionMessage message)
    {
        await _eventBus.PublishAsync(
            RoutingKeys.TaskReactionAdded,
            message);
    }
}