using FeeloryBackend.Models.DTOs.Auth;
using Microsoft.AspNetCore.Identity.Data;

namespace FeeloryBackend.Services.Interfaces;

public interface IAuthService
{
    // Register new user account
    Task<Guid> RegisterAsync(RegisterRequest request);

    // Authenticate user and return JWT token
    Task<string> LoginAsync(LoginRequest request);

    // Get current logged-in user information
    Task<UserDto> GetCurrentUserAsync(Guid userId);
}