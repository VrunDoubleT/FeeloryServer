using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using FeeloryBackend.Services.Implementations;
using FeeloryBackend.Services.Interfaces;

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
        services.AddScoped<IDayShareService, DayShareService>();
        services.AddScoped<IDayShareFeedService, DayShareFeedService>();
        return services;
    }
}