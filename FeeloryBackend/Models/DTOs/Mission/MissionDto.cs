using FeeloryBackend.Models.Enums;

namespace FeeloryBackend.Models.DTOs.Task;

public class MissionDto
{
    public Guid MissionId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int CurrentValue { get; set; }
    public int TargetValue { get; set; }
    public MissionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RewardClaimedAt { get; set; }
    public List<MissionRewardDto> Rewards { get; set; } = [];
}