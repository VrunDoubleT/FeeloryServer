namespace FeeloryBackend.Messaging.RabbitMQ.Queues;

public static class MissionQueues
{
    public const string Login = "mission.login.heartbeat";
    
    public const string LoginHistory = "mission.login.history";

    public const string DayShareCreated = "mission.dayshare.created";

    public const string ReactionSent = "mission.reaction.sent";

    public const string ReactionReceived = "mission.reaction.received";
    
    public const string UserCreated = "mission.user.created";
}