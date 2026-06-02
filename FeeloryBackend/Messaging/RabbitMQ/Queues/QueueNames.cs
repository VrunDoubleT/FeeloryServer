namespace FeeloryBackend.Messaging.RabbitMQ.Queues;

public static class QueueNames
{
    public const string Email = "email_queue";
    public const string Otp = "otp_queue";
    public const string PostCreated = "post_created_queue";
    public const string PostPermissionAdded = "post_permission_added_queue";
    public const string PostPermissionRemoved = "post_permission_removed_queue";
    public const string PostDeleted = "post_deleted_queue";
}