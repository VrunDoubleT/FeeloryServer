using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Routing;

namespace FeeloryBackend.Messaging.RabbitMQ.Publishers;

public class NotificationPublisher
{
    private readonly IEventBus _eventBus;

    public NotificationPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <summary>
    /// Publish friend request received message
    /// </summary>
    public async Task PublishFriendRequestReceivedAsync(FriendRequestReceivedMessage message)
    {
        await _eventBus.PublishAsync(NotificationRoutingKeys.FriendRequestReceived, message);
    }

    /// <summary>
    /// Publish friend request accepted message
    /// </summary>
    public async Task PublishFriendRequestAcceptedAsync(FriendRequestAcceptedMessage message)
    {
        await _eventBus.PublishAsync(NotificationRoutingKeys.FriendRequestAccepted, message);
    }

    /// <summary>
    /// Publish mission completed message
    /// </summary>
    public async Task PublishMissionCompletedAsync(MissionCompletedMessage message)
    {
        await _eventBus.PublishAsync(NotificationRoutingKeys.MissionCompleted, message);
    }

    /// <summary>
    /// Publish gift received message
    /// </summary>
    public async Task PublishGiftReceivedAsync(GiftReceivedMessage message)
    {
        await _eventBus.PublishAsync(NotificationRoutingKeys.GiftReceived, message);
    }
}