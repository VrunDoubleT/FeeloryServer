using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Models.DTOs.Reaction;

public class PostReactionDto
{
    public Guid PostId { get; set; }

    // User's own reaction (viewer mode)
    public EmoteDto? UserEmote { get; set; }

    // Aggregated reactions (owner mode)
    public List<ReactionGroupDto>? Groups { get; set; }
}