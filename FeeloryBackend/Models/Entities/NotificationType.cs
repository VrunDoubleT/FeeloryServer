namespace FeeloryBackend.Models.Entities;

public class NotificationType
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
 
    // Navigation properties
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}