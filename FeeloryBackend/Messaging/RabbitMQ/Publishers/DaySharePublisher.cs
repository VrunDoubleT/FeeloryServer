using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

public class DaySharePublisher
{
    private readonly IEventBus _eventBus;

    public DaySharePublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task PublishAsync(DayShareFeedMessage message)
    {
        // Chọn đúng routing key theo action
        var routingKey = message.Action switch
        {
            DayShareFeedMessage.ActionAdded   => RoutingKeys.DayShareAdded,
            DayShareFeedMessage.ActionRemoved => RoutingKeys.DayShareRemoved,
            DayShareFeedMessage.ActionDeleted => RoutingKeys.DayShareDeleted,
            _ => throw new ArgumentException($"Unknown action: {message.Action}")
        };

        await _eventBus.PublishAsync(routingKey, message);
    }
}