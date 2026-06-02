namespace FeeloryBackend.Messaging.RabbitMQ.Routing;

public class RoutingKeys
{
    public const string EmailSend = "email.send";
    public const string PostCreated = "post.created";
    public const string PostPermissionAdded = "post.permission.added";
    public const string PostPermissionRemoved = "post.permission.removed";
    public const string PostDeleted = "post.deleted";
}