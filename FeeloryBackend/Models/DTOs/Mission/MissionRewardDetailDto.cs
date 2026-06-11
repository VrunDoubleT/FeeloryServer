using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Models.DTOs.Task;

public class MissionRewardDetailDto : MissionRewardDto
{
    public List<EmoteDto> Emotes { get; set; } = [];
}