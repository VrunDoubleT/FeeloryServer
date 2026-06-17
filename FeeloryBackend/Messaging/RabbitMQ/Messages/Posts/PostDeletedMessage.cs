namespace FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;

public class PostDeletedMessage
{
    public Guid PostId { get; set; }
}