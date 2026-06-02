using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Models.DTOs.Post;

public class PostDto
{
    public Guid Id { get; set; }
    public UserDto User { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string? Description { get; set; }
    public string Privacy { get; set; } = null!;
    public EmoteDto MoodEmote { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ReactionCount { get; set; }
}