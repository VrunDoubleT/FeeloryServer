namespace FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;

public class PostUpdatedMessage
{
    public Guid AuthorId { get; set; }
    
    public Guid PostId { get; set; }

    public IReadOnlyCollection<Guid> AddedViewerIds { get; set; } = [];

    public IReadOnlyCollection<Guid> RemovedViewerIds { get; set; }   = [];
}