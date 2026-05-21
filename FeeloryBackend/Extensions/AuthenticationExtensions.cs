using FeeloryBackend.Helpers;
using FeeloryBackend.Responses;

namespace FeeloryBackend.Extensions;

using FeeloryBackend.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind Jwt settings from appsettings.json
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        var jwt = configuration.GetSection("Jwt").Get<JwtSettings>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Validate token issuer
                    ValidateIssuer = true,
                    ValidIssuer = jwt!.Issuer,

                    // Validate token audience
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,

                    // Validate token lifetime
                    ValidateLifetime = true,

                    // Validate token signature key
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.Key)
                    ),

                    // No tolerance for token expiration
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Stop default response behavior
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var response = new ApiErrorResponse("Unauthorized");
                        await context.Response.WriteAsync(
                            JsonSerializer.Serialize(response, JsonOptionsHelper.Default)
                        );
                    },

                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = new ApiErrorResponse("Forbidden");

                        await context.Response.WriteAsync(
                            JsonSerializer.Serialize(response, JsonOptionsHelper.Default)
                        );
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}