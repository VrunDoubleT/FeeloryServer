using FeeloryBackend.Models.Enums;

namespace FeeloryBackend.Models.DTOs.Task;

public class MissionDetailDto : MissionDto
{
    public List<MissionRewardDetailDto> Rewards { get; set; } = [];
}