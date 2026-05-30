namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class ReactionMessage
{
    public const string ActionPostReacted = "POST_REACTED";

    public string Action { get; set; } = string.Empty;
    public Guid TargetOwnerId { get; set; }
    public Guid ReactorId { get; set; }
    public string ReactorName { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
}