namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class FriendRequestAcceptedMessage
{
    // User who accepted the request
    public Guid AccepterId { get; set; }

    // Original sender
    public Guid SenderId { get; set; }

    // Friend request identifier
    public Guid FriendRequestId { get; set; }
}