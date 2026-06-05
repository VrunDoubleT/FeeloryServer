namespace FeeloryBackend.Messaging.RabbitMQ.Routing;

public class RoutingKeys
{
    public const string EmailSend = "email.send";
    public const string PostCreated = "post.created";
    public const string PostPermissionAdded = "post.permission.added";
    public const string PostPermissionRemoved = "post.permission.removed";
    public const string PostDeleted = "post.deleted";
    public const string DayShareCreated = "dayshare.created";
    public const string DayShareAdded   = "dayshare.added";
    public const string DayShareRemoved = "dayshare.removed";
    public const string DayShareDeleted = "dayshare.deleted";
}