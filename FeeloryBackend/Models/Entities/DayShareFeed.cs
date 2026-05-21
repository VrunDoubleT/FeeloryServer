namespace FeeloryBackend.Models.Entities;

public class DayShareFeed
{
    public Guid Id { get; set; }
    public Guid DayShareId { get; set; }
    public Guid ViewerId { get; set; }
    public DateTime PostedAt { get; set; }
    
    public DayShare DayShare { get; set; } = null!;
    public User Viewer { get; set; } = null!;
}