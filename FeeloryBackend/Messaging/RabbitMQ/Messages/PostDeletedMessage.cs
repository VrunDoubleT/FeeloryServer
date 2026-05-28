namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class PostDeletedMessage
{
    public Guid PostId { get; set; }
}