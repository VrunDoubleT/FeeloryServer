namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class DayShareFeedMessage
{
      public const string ActionAdded   = "ADDED";
      public const string ActionRemoved = "REMOVED";
      public const string ActionDeleted = "DELETED";
        
    public string Action { get; set; } = string.Empty;
    public Guid DayShareId { get; set; }
    public List<Guid> ViewerIds { get; set; } = new();
}