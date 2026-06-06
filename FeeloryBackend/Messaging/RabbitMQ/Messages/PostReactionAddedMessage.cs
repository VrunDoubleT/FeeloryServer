namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class PostReactionAddedMessage
{
    // Reacted post identifier
    public Guid PostId { get; set; }

    // User who reacted
    public Guid ReactorId { get; set; }

    // Owner of the post
    public Guid OwnerId { get; set; }

    // Reaction code
    public Guid EmoteId { get; set; }
}