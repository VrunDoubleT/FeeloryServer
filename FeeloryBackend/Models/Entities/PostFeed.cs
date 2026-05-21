namespace FeeloryBackend.Models.Entities;

public class PostFeed
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid ViewerId { get; set; }
    public DateTime PostedAt { get; set; }
    
    // Navigation properties
    public Post Post { get; set; } = null!;
    public User Viewer { get; set; } = null!;
}