using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Models.DTOs.Reaction;

public class ReactionDto
{
    public Guid Id { get; set; }
    public UserDto User { get; set; } = null!;
    public EmoteDto Emote { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}