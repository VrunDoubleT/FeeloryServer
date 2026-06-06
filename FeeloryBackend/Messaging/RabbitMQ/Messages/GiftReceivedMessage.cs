namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class GiftReceivedMessage
{
    // Receiver of the gift
    public Guid UserId { get; set; }

    // Gift identifier
    public Guid GiftId { get; set; }

    // Gift name
    public string GiftName { get; set; } = null!;
}