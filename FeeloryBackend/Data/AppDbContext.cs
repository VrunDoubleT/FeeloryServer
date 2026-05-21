using FeeloryBackend.Data.Configurations;
using FeeloryBackend.Models.Entities;
using Task = FeeloryBackend.Models.Entities.Task;

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
    public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();

    // =======================
    // GAMIFICATION / TASK SYSTEM
    // =======================
    public DbSet<Task> Tasks => Set<Task>();
    public DbSet<TaskType> TaskTypes => Set<TaskType>();
    public DbSet<TaskReward> TaskRewards => Set<TaskReward>();
    public DbSet<UserTaskProgress> UserTaskProgresses => Set<UserTaskProgress>();

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
        modelBuilder.ApplyConfiguration(new NotificationTypeConfiguration());

        // =======================
        // TASK SYSTEM
        // =======================
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new TaskTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TaskRewardConfiguration());
        modelBuilder.ApplyConfiguration(new UserTaskProgressConfiguration());

        // =======================
        // USER ACTIVITY
        // =======================
        modelBuilder.ApplyConfiguration(new UserLoginHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new UserPackageConfiguration());
    }
}