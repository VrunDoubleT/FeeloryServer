namespace FeeloryBackend.Models.DTOs.Post;

public class CreatePostRequestDto
{
    public IFormFile Image { get; set; } = null!;
    public string? Description { get; set; }
    public Guid MoodEmoteId { get; set; }
    public string Privacy { get; set; } = null!;
    public List<Guid>? AllowedUserIds { get; set; }
}