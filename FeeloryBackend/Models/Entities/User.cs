namespace FeeloryBackend.Models.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<FriendRequest> SentFriendRequests { get; set; } = new List<FriendRequest>();
    public ICollection<FriendRequest> ReceivedFriendRequests { get; set; } = new List<FriendRequest>();
    public ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<PostViewer> PostViewers { get; set; } = new List<PostViewer>();
    public ICollection<PostFeed> PostFeeds { get; set; } = new List<PostFeed>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    public ICollection<DayShare> DayShares { get; set; } = new List<DayShare>();
    public ICollection<DayShareViewer> DayShareViewers { get; set; } = new List<DayShareViewer>();
    public ICollection<UserLoginHistory> LoginHistories { get; set; } = new List<UserLoginHistory>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<UserMission> UserMissions { get; set; } = new List<UserMission>();
    public ICollection<UserMissionReactionHistory> MissionReactionHistories { get; set; } = new List<UserMissionReactionHistory>();
    public ICollection<UserMissionReactionHistory> ReactedMissionHistories { get; set; } = new List<UserMissionReactionHistory>();
    
    // Tách 2 collection rõ ràng theo canonical direction
    public ICollection<Friend> FriendsAsUser   { get; set; } = [];
    public ICollection<Friend> FriendsAsFriend { get; set; } = [];
}