namespace FeeloryBackend.Messaging.RabbitMQ.Routing;

public class RoutingKeys
{
    public const string EmailSend = "email.send";

    public const string Reaction = "reaction.notify";
    public const string TaskReactionAdded = "task.reaction.added";

}