namespace FeeloryBackend.Models.Entities;

public class PostViewer
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid ViewerId { get; set; }
    
    // Navigation properties
    public Post Post { get; set; } = null!;
    public User Viewer { get; set; } = null!;
}