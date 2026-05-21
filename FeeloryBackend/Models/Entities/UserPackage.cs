namespace FeeloryBackend.Models.Entities;

public class UserPackage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PackageId { get; set; }
    public DateTime UnlockedAt { get; set; }
 
    // Navigation properties
    public User User { get; set; } = null!;
    public EmotePackage Package { get; set; } = null!;
}