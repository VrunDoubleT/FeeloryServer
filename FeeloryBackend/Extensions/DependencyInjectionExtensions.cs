using FeeloryBackend.Services.Implementations;
using FeeloryBackend.Services.Interfaces;
using FeeloryBackend.Utils;

namespace FeeloryBackend.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        
        // Register custom services
        
        // Auth
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<GenerateUniqueUserName>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        // Email
        services.AddScoped<IEmailService, EmailService>();
        // Cloudinary
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        // Friend
        services.AddScoped<IFriendService, FriendService>();
        // Post
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPostFeedService, PostFeedService>();
        services.AddScoped<IPostAccessService, PostAccessService>();
        // Dayshare
        services.AddScoped<IDayShareAccessService, DayShareAccessService>();
        services.AddScoped<IDayShareService, DayShareService>();
        services.AddScoped<IDayShareFeedService, DayShareFeedService>();
        // Calendar
        services.AddScoped<ICalendarService, CalendarService>();
        // Emote
        services.AddScoped<IEmoteService, EmoteService>();
        services.AddScoped<IEmotePackageService, EmotePackageService>();
        //Reaction
        services.AddScoped<IReactionService, ReactionService>();
        // Notification
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationCreatorService, NotificationCreatorService>();
        // Missions
        services.AddScoped<IMissionProgressService, MissionProgressService>();
        // Heartbeat
        services.AddScoped<IHeartbeatService, HeartbeatService>();
        
        return services;
    }
}