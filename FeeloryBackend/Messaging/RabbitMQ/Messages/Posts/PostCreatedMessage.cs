namespace FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;

public class PostCreatedMessage
{
    // Created post identifier
    public Guid PostId { get; set; }

    // Author of the post
    public Guid AuthorId { get; set; }

    // Users allowed to view the post
    public IReadOnlyCollection<Guid> RecipientIds { get; set; } = [];
}