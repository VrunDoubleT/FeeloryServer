namespace FeeloryBackend.Models.Entities;

public class DayShareViewer
{
    public Guid Id { get; set; }
    public Guid DayShareId { get; set; }
    public Guid ViewerId { get; set; }
 
    // Navigation properties
    public DayShare DayShare { get; set; } = null!;
    public User Viewer { get; set; } = null!;
}