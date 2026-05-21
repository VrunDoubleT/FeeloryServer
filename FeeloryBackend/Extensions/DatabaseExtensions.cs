using FeeloryBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register AppDbContext and configure SQL Server connection string
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        );

        return services;
    }
}