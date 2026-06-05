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
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IFriendService, FriendService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPostFeedService, PostFeedService>();
        
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<GenerateUniqueUserName>();
        services.AddScoped<IDayShareService, DayShareService>();
        services.AddScoped<IDayShareFeedService, DayShareFeedService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmoteService, EmoteService>();
        services.AddScoped<IEmotePackageService, EmotePackageService>();

        services.AddScoped<IReactionService, ReactionService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}