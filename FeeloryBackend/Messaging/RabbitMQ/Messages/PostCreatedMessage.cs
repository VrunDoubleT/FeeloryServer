namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class PostCreatedMessage
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Privacy { get; set; } = null!;
    public List<Guid>? AllowedUserIds { get; set; }
    public DateTime CreatedAt { get; set; }
}