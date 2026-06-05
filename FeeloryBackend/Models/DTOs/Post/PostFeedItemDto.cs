using FeeloryBackend.Models.DTOs.Auth;

namespace FeeloryBackend.Models.DTOs.Post;

public class PostFeedItemDto
{
    public PostDto Post { get; set; } = null!;
    public UserDto Owner { get; set; } = null!;
}