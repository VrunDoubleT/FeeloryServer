namespace FeeloryBackend.Models.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TypeId { get; set; }
    public string Title { get; set; } = null!;
    public string? Message { get; set; }
    public string? DataJson { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
 
    // Navigation properties
    public User User { get; set; } = null!;
    public NotificationType Type { get; set; } = null!;
}