using FeeloryBackend.Commons;
using FeeloryBackend.Responses;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Extensions;

public static class ApiBehaviorExtensions
{
    public static IServiceCollection AddCustomModelValidationResponse(
        this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var response = new ApiErrorResponse(
                    message: "Validation failed",
                    errors: errors
                );

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}