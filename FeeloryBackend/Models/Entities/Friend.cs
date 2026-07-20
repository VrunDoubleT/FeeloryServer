namespace FeeloryBackend.Models.Entities;

public class Friend
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FriendId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public User FriendUser { get; set; } = null!;
    
    // Factory method — đảm bảo canonical order, không để caller tự set
    public static Friend Create(Guid userA, Guid userB)
    {
        if (userA == userB)
            throw new ArgumentException("Cannot friend yourself.");

        return new Friend
        {
            Id = Guid.NewGuid(),
            UserId = userA,
            FriendId = userB,
            CreatedAt = DateTime.UtcNow
        };
    }
}