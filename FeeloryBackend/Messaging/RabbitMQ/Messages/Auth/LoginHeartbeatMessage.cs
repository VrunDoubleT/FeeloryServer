namespace FeeloryBackend.Messaging.RabbitMQ.Messages.Auth;

public class LoginHeartbeatMessage
{
    public Guid UserId { get; set; }

    public DateOnly LoginDate { get; set; }
}