namespace FeeloryBackend.Messaging.RabbitMQ.Routing;

public class RoutingKeys
{
    public const string EmailSend = "email.send";
    public const string PostCreated = "post.created";
    public const string PostPermissionChanged = "post.permission.changed";
    public const string PostDeleted = "post.deleted";
}