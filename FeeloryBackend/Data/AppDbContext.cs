using FeeloryBackend.Data.Configurations;
using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // =======================
    // DB SETS - CORE SYSTEM
    // =======================
    public DbSet<User> Users => Set<User>();
    public DbSet<Friend> Friends => Set<Friend>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();

    // =======================
    // POSTS SYSTEM
    // =======================
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostFeed> PostFeeds => Set<PostFeed>();
    public DbSet<PostViewer> PostViewers => Set<PostViewer>();
    public DbSet<Reaction> Reactions => Set<Reaction>();

    // =======================
    // DAY SHARE SYSTEM
    // =======================
    public DbSet<DayShare> DayShares => Set<DayShare>();
    public DbSet<DaySharePost> DaySharePosts => Set<DaySharePost>();
    public DbSet<DayShareViewer> DayShareViewers => Set<DayShareViewer>();
    public DbSet<DayShareFeed> DayShareFeeds => Set<DayShareFeed>();

    // =======================
    // EMOTE SYSTEM
    // =======================
    public DbSet<Emote> Emotes => Set<Emote>();
    public DbSet<EmotePackage> EmotePackages => Set<EmotePackage>();
    public DbSet<EmotePackageItem> EmotePackageItems => Set<EmotePackageItem>();

    // =======================
    // NOTIFICATION SYSTEM
    // =======================
    public DbSet<Notification> Notifications => Set<Notification>();

    // ===============================
    // GAMIFICATION / MISSION SYSTEM
    // ===============================
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<MissionType> MissionTypes => Set<MissionType>();
    public DbSet<MissionReward> MissionRewards => Set<MissionReward>();
    public DbSet<UserMission> UserMissions => Set<UserMission>();
    public DbSet<UserMissionReactionHistory> UserMissionReactionHistories => Set<UserMissionReactionHistory>();

    // =======================
    // USER ACTIVITY
    // =======================
    public DbSet<UserLoginHistory> UserLoginHistories => Set<UserLoginHistory>();
    public DbSet<UserPackage> UserPackages => Set<UserPackage>();

    // =======================
    // MODEL CONFIGURATION
    // =======================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =======================
        // USER & FRIEND SYSTEM
        // =======================
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new FriendConfiguration());
        modelBuilder.ApplyConfiguration(new FriendRequestConfiguration());

        // =======================
        // POST SYSTEM
        // =======================
        modelBuilder.ApplyConfiguration(new PostConfiguration());
        modelBuilder.ApplyConfiguration(new PostFeedConfiguration());
        modelBuilder.ApplyConfiguration(new PostViewerConfiguration());
        modelBuilder.ApplyConfiguration(new ReactionConfiguration());

        // =======================
        // DAY SHARE SYSTEM
        // =======================
        modelBuilder.ApplyConfiguration(new DayShareConfiguration());
        modelBuilder.ApplyConfiguration(new DaySharePostConfiguration());
        modelBuilder.ApplyConfiguration(new DayShareViewerConfiguration());
        modelBuilder.ApplyConfiguration(new DayShareFeedConfiguration()); 
        // =======================
        // EMOTE SYSTEM
        // =======================
        modelBuilder.ApplyConfiguration(new EmoteConfiguration());
        modelBuilder.ApplyConfiguration(new EmotePackageConfiguration());
        modelBuilder.ApplyConfiguration(new EmotePackageItemConfiguration());

        // =======================
        // NOTIFICATION SYSTEM
        // =======================
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());

        // =======================
        // TASK SYSTEM
        // =======================
        modelBuilder.ApplyConfiguration(new MissionConfiguration());
        modelBuilder.ApplyConfiguration(new MissionTypeConfiguration());
        modelBuilder.ApplyConfiguration(new MissionRewardConfiguration());
        modelBuilder.ApplyConfiguration(new UserMissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserMissionReactionHistoryConfiguration());

        // =======================
        // USER ACTIVITY
        // =======================
        modelBuilder.ApplyConfiguration(new UserLoginHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new UserPackageConfiguration());
    }
}