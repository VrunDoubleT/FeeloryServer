using FeeloryBackend.Middlewares;

namespace FeeloryBackend.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalException(this IApplicationBuilder app)
    {
        // Register global exception middleware
        app.UseMiddleware<GlobalExceptionMiddleware>();

        return app;
    }
}