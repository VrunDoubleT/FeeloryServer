namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class GiftReceivedMessage
{
    // Receiver of the gift
    public Guid UserId { get; set; }

    // Gift identifier
    public Guid MissionId { get; set; }
}