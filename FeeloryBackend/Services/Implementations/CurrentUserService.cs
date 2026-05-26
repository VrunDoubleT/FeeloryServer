using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Services.Implementations;

using System.Security.Claims;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        // Extract user id from JWT claim
        var userIdString = _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdString))
            throw new UnauthorizedAccessException("User is not authenticated");

        return Guid.Parse(userIdString);
    }
}