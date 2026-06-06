namespace FeeloryBackend.Messaging.RabbitMQ.Routing;

public class RoutingKeys
{
    public const string EmailSend = "email.send";
    
    // Post
    public const string PostPermissionAdded = "post.permission.added";
    public const string PostPermissionRemoved = "post.permission.removed";
    public const string PostDeleted = "post.deleted";
    
    // Dayshare
    public const string DayShareAdded   = "dayshare.added";
    public const string DayShareRemoved = "dayshare.removed";
    public const string DayShareDeleted = "dayshare.deleted";
    
    // Reaction
    public const string Reaction = "reaction.notify";
}