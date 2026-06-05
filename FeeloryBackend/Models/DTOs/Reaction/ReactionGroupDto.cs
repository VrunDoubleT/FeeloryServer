using FeeloryBackend.Models.DTOs.User;
using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Models.DTOs.Reaction;

public class ReactionGroupDto
{
    public EmoteDto Emote { get; set; } = null!;
    public int Count { get; set; }
    public List<ReactionUserDto> Users { get; set; } = new();
}