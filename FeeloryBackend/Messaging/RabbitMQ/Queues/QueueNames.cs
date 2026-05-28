namespace FeeloryBackend.Messaging.RabbitMQ.Queues;

public static class QueueNames
{
    public const string Email = "email_queue";
    public const string Otp = "otp_queue";
    public const string PostCreated = "post_created_queue";
    public const string PostPermission = "post_permission_queue";
    public const string PostDeleted = "post_deleted_queue";
}