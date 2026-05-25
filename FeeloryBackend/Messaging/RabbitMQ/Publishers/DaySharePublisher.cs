using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

public class DaySharePublisher
{
    private readonly IEventBus _eventBus;

    public DaySharePublisher(
        IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task PublishAsync(
        DayShareFeedMessage message)
    {
        await _eventBus.PublishAsync(
            RoutingKeys.DayShareFeed,
            message);
    }
}