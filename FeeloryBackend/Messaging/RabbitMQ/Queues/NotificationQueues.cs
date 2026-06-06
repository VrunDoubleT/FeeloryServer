namespace FeeloryBackend.Messaging.RabbitMQ.Queues;

public static class NotificationQueues
{
    // Post notifications
    public const string PostCreated = "notification.post.created.queue";

    public const string PostReactionAdded = "notification.post.reaction.added.queue";

    // DayShare notifications
    public const string DayShareCreated = "notification.dayshare.created.queue";

    // Friend notifications
    public const string FriendRequestReceived = "notification.friend.request.received.queue";

    public const string FriendRequestAccepted = "notification.friend.request.accepted.queue";

    // Mission notifications
    public const string MissionCompleted = "notification.mission.completed.queue";

    // Gift notifications
    public const string GiftReceived = "notification.gift.received.queue";
}