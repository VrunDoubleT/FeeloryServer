namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class DayShareCreatedMessage
{
    // Created DayShare identifier
    public Guid DayShareId { get; set; }

    // Author of the DayShare
    public Guid AuthorId { get; set; }

    // Users allowed to view the DayShare
    public IReadOnlyCollection<Guid> RecipientIds { get; set; } = [];
}