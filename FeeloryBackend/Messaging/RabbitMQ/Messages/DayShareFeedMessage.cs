namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class DayShareFeedMessage
{
    public string Action { get; set; } = string.Empty;
    public Guid DayShareId { get; set; }
    public List<Guid> ViewerIds { get; set; } = new();
}