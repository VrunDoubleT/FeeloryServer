using FeeloryBackend.Services.Implementations;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IFriendService, FriendService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}