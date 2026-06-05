using FeeloryBackend.Models.DTOs.Auth;

namespace FeeloryBackend.Models.DTOs.Post;

public class PostDetailDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? Description { get; set; }
    public string Privacy { get; set; } = null!;
    public string MoodEmote { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public UserDto Owner { get; set; } = null!;
    public List<PostReactionDto> Reactions { get; set; } = [];
}