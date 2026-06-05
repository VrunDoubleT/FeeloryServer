namespace FeeloryBackend.Models.DTOs.Post;

public class PostReactionDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string ReactionName { get; set; } = null!;
    public string Icon { get; set; } = null!;
}