using FeeloryBackend.Settings;

namespace FeeloryBackend.Extensions;

public static class EmailExtensions
{
    public static IServiceCollection AddEmailService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind config
        services.Configure<EmailSettings>(
            configuration.GetSection("EmailSettings"));
        
        return services;
    }
}