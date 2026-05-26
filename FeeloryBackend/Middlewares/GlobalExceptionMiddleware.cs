using FeeloryBackend.Helpers;

namespace FeeloryBackend.Middlewares;

using System.Net;
using System.Text.Json;
using FeeloryBackend.Responses;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    
    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var apiErrorResponse = new ApiErrorResponse("Internal Server Error");

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(apiErrorResponse, JsonOptionsHelper.Default)
            );
        }
    }
}