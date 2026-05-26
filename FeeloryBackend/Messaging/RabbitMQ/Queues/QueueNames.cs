namespace FeeloryBackend.Messaging.RabbitMQ.Queues;

public static class QueueNames
{
    public const string Email = "email_queue";
    public const string Otp = "otp_queue";
    public const string DayShareCreated = "dayshare_created_queue";
    public const string DayShareAdded   = "dayshare_added_queue";
    public const string DayShareRemoved = "dayshare_removed_queue";
    public const string DayShareDeleted = "dayshare_deleted_queue";
}