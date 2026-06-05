namespace FeeloryBackend.Messaging.RabbitMQ.Queues;

public static class QueueNames
{
    // Email
    public const string Email = "email_queue";
    // Post
    public const string PostPermissionAdded = "post_permission_added_queue";
    public const string PostPermissionRemoved = "post_permission_removed_queue";
    public const string PostDeleted = "post_deleted_queue";
    // DayShare
    public const string DayShareCreated = "dayshare_created_queue";
    public const string DayShareAdded   = "dayshare_added_queue";
    public const string DayShareRemoved = "dayshare_removed_queue";
    public const string DayShareDeleted = "dayshare_deleted_queue";
    //Reaction
    public const string Reaction = "reaction_queue";
}