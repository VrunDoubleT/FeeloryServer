namespace FeeloryBackend.Models.DTOs.Post;

public class MyPostItemDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? Description { get; set; }
    public string Privacy { get; set; } = null!;
    public string MoodEmote { get; set; } = null!;
    public int ReactionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}