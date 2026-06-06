namespace FeeloryBackend.Messaging.RabbitMQ.Routing;

public static class NotificationRoutingKeys
{
    // Post
    public const string PostCreated = "notification.post.created";
    public const string PostReactionAdded = "notification.post.reaction.added";

    // DayShare
    public const string DayShareCreated = "notification.dayshare.created";

    // Friend
    public const string FriendRequestReceived = "notification.friend.request.received";
    public const string FriendRequestAccepted = "notification.friend.request.accepted";

    // Mission
    public const string MissionCompleted = "notification.mission.completed";

    // Gift
    public const string GiftReceived = "notification.gift.received";
}