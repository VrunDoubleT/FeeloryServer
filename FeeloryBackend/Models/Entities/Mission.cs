namespace FeeloryBackend.Models.Entities;

public class Mission
{
    public Guid Id { get; set; }

    public Guid MissionTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int TargetValue { get; set; }

    // Number of days allowed
    public int DurationDays { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties

    public MissionType MissionType { get; set; } = null!;

    public ICollection<MissionReward> Rewards { get; set; } = new List<MissionReward>();
    
    public ICollection<UserMission> UserMissions { get; set; } = new List<UserMission>();

    public ICollection<UserMissionReactionHistory> ReactionHistories { get; set; } = new List<UserMissionReactionHistory>();
}