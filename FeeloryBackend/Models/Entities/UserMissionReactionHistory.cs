namespace FeeloryBackend.Models.Entities;

public class UserMissionReactionHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public Guid MissionId { get; set; }

    public Guid PostId { get; set; }
    
    public Guid? ReactorId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Mission Mission { get; set; } = null!;
    public Post Post { get; set; } = null!;
    public User Reactor { get; set; } = null!;
}