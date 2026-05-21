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
}