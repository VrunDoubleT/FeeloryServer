namespace FeeloryBackend.Extensions;

using FeeloryBackend.Settings;

public static class RedisExtensions
{
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));

        var redis = configuration.GetSection("Redis").Get<RedisSettings>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redis!.ConnectionString;
        });

        return services;
    }
}