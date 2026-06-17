using FeeloryBackend.Models.Enums;

namespace FeeloryBackend.Models.Entities;

public class UserMission
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid MissionId { get; set; }

    public int CurrentValue { get; set; }

    public MissionStatus Status { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime ExpiredAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? RewardClaimedAt { get; set; }

    public User User { get; set; } = null!;

    public Mission Mission { get; set; } = null!;
}