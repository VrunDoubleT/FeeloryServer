namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class PostMessage
{
    public const string ActionCreated = "CREATED";
    public const string ActionAdded = "ADDED";
    public const string ActionRemoved = "REMOVED";
    public const string ActionDeleted = "DELETED";

    public string Action { get; set; } = string.Empty;
    public Guid PostId { get; set; }
    public List<Guid> ViewerIds { get; set; } = [];
}
