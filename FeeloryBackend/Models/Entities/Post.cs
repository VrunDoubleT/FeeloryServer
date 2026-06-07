using FeeloryBackend.Constants;

namespace FeeloryBackend.Models.Entities;

public class Post
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid MoodEmoteId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string Description { get; set; }
    public string Privacy { get; set; } = PostPrivacyConstants.Private;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Emote MoodEmote { get; set; } = null!;
    public ICollection<PostViewer> PostViewers { get; set; } = new List<PostViewer>();
    public ICollection<PostFeed> PostFeeds { get; set; } = new List<PostFeed>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    public ICollection<DaySharePost> DaySharePosts { get; set; } = new List<DaySharePost>();
    public ICollection<UserMissionReactionHistory> ReactionMissionHistories { get; set; } = new List<UserMissionReactionHistory>();
}