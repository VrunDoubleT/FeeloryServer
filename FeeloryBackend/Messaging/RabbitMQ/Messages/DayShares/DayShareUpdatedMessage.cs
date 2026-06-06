namespace FeeloryBackend.Messaging.RabbitMQ.Messages.DayShares;

public class DayShareUpdatedMessage
{
    public Guid AuthorId { get; set; }
    
    public Guid DayShareId { get; set; }

    public IReadOnlyCollection<Guid> AddedViewerIds { get; set; } = [];

    public IReadOnlyCollection<Guid> RemovedViewerIds { get; set; }   = [];
}