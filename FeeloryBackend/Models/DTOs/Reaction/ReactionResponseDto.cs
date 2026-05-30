using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Models.DTOs.Reaction;

public class ReactionResponseDto
{
    public Guid ReactionId { get; set; }

    public EmoteDto Emote { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}