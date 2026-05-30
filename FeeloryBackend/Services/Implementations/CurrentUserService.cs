// Services/Implementations/CurrentUserService.cs
using System.Security.Claims;
using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var value = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(value))
            throw new UnauthorizedAccessException("User is not authenticated.");

        return Guid.Parse(value);
    }
}