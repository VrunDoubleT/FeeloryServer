using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Models.DTOs.Post;

public class PostFeedItemDto
{
    public PostDto Post { get; set; } = null!;
    public EmoteDto? Emote { get; set; }
    public UserDto Owner { get; set; } = null!;
}