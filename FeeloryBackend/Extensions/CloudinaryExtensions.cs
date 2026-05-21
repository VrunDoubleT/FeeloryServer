using CloudinaryDotNet;
using FeeloryBackend.Services.Implementations;
using FeeloryBackend.Services.Interfaces;
using FeeloryBackend.Settings;
using Microsoft.Extensions.Options;

namespace FeeloryBackend.Extensions;

public static class CloudinaryExtensions
{
    public static IServiceCollection AddCloudinaryService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind settings
        services.Configure<CloudinarySettings>(
            configuration.GetSection("Cloudinary"));

        return services;
    }
}