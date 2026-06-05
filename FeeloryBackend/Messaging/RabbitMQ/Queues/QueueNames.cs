namespace FeeloryBackend.Messaging.RabbitMQ.Queues;

public static class QueueNames
{
    public const string Email = "email_queue";
    public const string Otp = "otp_queue";
    public const string PostCreated = "post_created_queue";
    public const string PostPermissionAdded = "post_permission_added_queue";
    public const string PostPermissionRemoved = "post_permission_removed_queue";
    public const string PostDeleted = "post_deleted_queue";
    public const string DayShareCreated = "dayshare_created_queue";
    public const string DayShareAdded   = "dayshare_added_queue";
    public const string DayShareRemoved = "dayshare_removed_queue";
    public const string DayShareDeleted = "dayshare_deleted_queue";

    public const string Reaction = "reaction_queue";
    public const string TaskReactionAdded = "task_reaction_added_queue";

}