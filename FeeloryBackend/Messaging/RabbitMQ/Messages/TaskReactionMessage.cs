namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class TaskReactionMessage
{
    public Guid UserId { get; set; }

    public Guid ReactionId { get; set; }

    public DateTime CreatedAt { get; set; }
}