namespace FeeloryBackend.Messaging.RabbitMQ.Messages;

public class MissionCompletedMessage
{
    // User receiving reward
    public Guid UserId { get; set; }

    // Completed mission identifier
    public Guid MissionId { get; set; }

    // Reward amount
    public int RewardCoin { get; set; }
}