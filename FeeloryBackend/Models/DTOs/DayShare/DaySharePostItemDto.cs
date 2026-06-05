using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.DTOs.Reaction;

namespace FeeloryBackend.Models.DTOs.DayShare;

public class DaySharePostItemDto
{
    public Guid Id { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public DayShareMoodEmoteDto? MoodEmote { get; set; }
    public DateTime CreatedAt { get; set; }
    public EmoteDto? MyReaction { get; set; }
    // public List<ReactionGroupDto>? Reactions { get; set; }
}