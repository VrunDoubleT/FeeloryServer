namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class PostPermissionChangedMessage
{
    public Guid PostId { get; set; }
    public List<Guid> AddedUserIds { get; set; } = [];
    public List<Guid> RemovedUserIds { get; set; } = [];
}