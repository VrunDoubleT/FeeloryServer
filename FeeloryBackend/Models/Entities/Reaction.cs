namespace FeeloryBackend.Models.Entities;

public class Reaction
{
    public Guid Id { get; set; }
    public Guid? PostId { get; set; }
    public Guid UserId { get; set; }
    public Guid EmoteId { get; set; }
    public DateTime CreatedAt { get; set; }
    

    // Navigation properties
    public Post? Post { get; set; } = null!;
    public User User { get; set; } = null!;
    public Emote Emote { get; set; } = null!;
    
}        