using FeeloryBackend.Messaging.RabbitMQ.Messages.Users;
using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

public class UserCreatedPublisher
{
    private readonly IEventBus _eventBus;

    public UserCreatedPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task PublishUserCreatedAsync(UserCreatedMessage message)
    {
        await _eventBus.PublishAsync(RoutingKeys.UserCreated, message);
    }
}