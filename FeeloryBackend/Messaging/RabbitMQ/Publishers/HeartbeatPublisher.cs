using FeeloryBackend.Messaging.RabbitMQ.Messages.Auth;
using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

public class HeartbeatPublisher
{
    private readonly IEventBus _eventBus;

    public HeartbeatPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task TrackLoginAsync(LoginHeartbeatMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.LoginHeartbeat, message);
    }
}