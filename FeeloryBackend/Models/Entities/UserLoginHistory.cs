namespace FeeloryBackend.Models.Entities;

public class UserLoginHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly LoginDate { get; set; }
    public DateTime CreatedAt { get; set; }
 
    // Navigation properties
    public User User { get; set; } = null!;
}