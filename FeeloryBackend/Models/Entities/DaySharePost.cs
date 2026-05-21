namespace FeeloryBackend.Models.Entities;

public class DaySharePost
{
    public Guid Id { get; set; }
    public Guid DayShareId { get; set; }
    public Guid PostId { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public DayShare DayShare { get; set; } = null!;
    public Post Post { get; set; } = null!;
}