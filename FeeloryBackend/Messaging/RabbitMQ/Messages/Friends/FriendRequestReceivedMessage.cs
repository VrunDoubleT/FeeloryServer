namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class FriendRequestReceivedMessage
{
    // Sender of friend request
    public Guid SenderId { get; set; }

    // Receiver of friend request
    public Guid ReceiverId { get; set; }

    // Friend request identifier
    public Guid FriendRequestId { get; set; }
}