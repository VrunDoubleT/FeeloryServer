using FeeloryBackend.Models.Enums;

namespace FeeloryBackend.Models.Entities;

public class Notification
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }

    public Guid? ActorId { get; set; }
    
    public NotificationType Type { get; set; }
    
    public Guid? TargetId { get; set; }
    
    public string? DataJson { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    // Navigation properties

    public User User { get; set; } = null!;

    public User? Actor { get; set; }
}