using FeeloryBackend.Models.Entities;
using FeeloryBackend.Responses;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Services.Interfaces;

public interface IRefreshTokenService
{
    RefreshTokenResponse GenerateRefreshToken(Guid userId);

    Task SaveRefreshTokenAsync(
        string refreshToken,
        Guid userId,
        DateTime expiredAt);

    Task<RefreshTokenData?> GetRefreshTokenAsync(string refreshToken);

    Task RemoveRefreshTokenAsync(string refreshToken);

    Task<RefreshTokenResponse?> RotateRefreshTokenAsync(string oldRefreshToken);

    // Revoke all refresh tokens for a user by incrementing token version
    Task RevokeAllUserTokensAsync(Guid userId);

    // Get the current token version for a user
    Task<long> GetCurrentTokenVersionAsync(Guid userId);
}