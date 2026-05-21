using FeeloryBackend.Constants;

namespace FeeloryBackend.Models.Entities;

public class DayShare
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Description { get; set; }
    public DateTime SharedDate { get; set; }
    public string ShareType { get; set; } = DayShareTypeConstants.Friends;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<DaySharePost> DaySharePosts { get; set; } = new List<DaySharePost>();
    public ICollection<DayShareFeed> DayShareFeeds { get; set; } = new List<DayShareFeed>();
    public ICollection<DayShareViewer> DayShareViewers { get; set; } = new List<DayShareViewer>();
}