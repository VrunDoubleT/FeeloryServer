namespace FeeloryBackend.Messaging.RabbitMQ.Routing;

public static class NotificationRoutingKeys
{
    // Friend
    public const string FriendRequestReceived = "notification.friend.request.received";
    public const string FriendRequestAccepted = "notification.friend.request.accepted";

    // Mission
    public const string MissionCompleted = "notification.mission.completed";

    // Gift
    public const string GiftReceived = "notification.gift.received";
}