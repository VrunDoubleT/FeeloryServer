namespace FeeloryBackend.Messaging.RabbitMQ.Routing;

public static class RoutingKeys
{
    // Email
    public const string EmailSend = "email.send";
    
    // Post
    public const string PostCreated = "post.created";
    public const string PostUpdated = "post.updated";
    public const string PostDeleted = "post.deleted";
    
    // Dayshare
    public const string DayShareCreated   = "dayshare.created";
    public const string DayShareUpdated = "dayshare.updated";
    public const string DayShareDeleted = "dayshare.deleted";
    
    // Reaction
    public const string PostReactionAdded = "post.reaction_added";
    
    // Auth
    public const string LoginHeartbeat = "login.heartbeat";
}