using FeeloryBackend.Helpers;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Services.Implementations;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FeeloryBackend.Responses;
using FeeloryBackend.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly JwtSettings _jwt;
    private readonly IDistributedCache _cache;
    public RefreshTokenService(
        IOptions<JwtSettings> jwtOptions,
        IDistributedCache cache
        )
    {
        _jwt = jwtOptions.Value;
        _cache = cache;
    }
    
    // ─── Token Version (bulk revocation) ────────────────────────────────────

    public async Task<long> GetCurrentTokenVersionAsync(Guid userId)
    {
        var versionStr = await _cache.GetStringAsync(HashedToken.GetVersionKey(userId));
        return long.TryParse(versionStr, out var version) ? version : 0;
    }
    
    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var currentVersion = await GetCurrentTokenVersionAsync(userId);
        var newVersion = currentVersion + 1;
        
        await _cache.SetStringAsync(
            HashedToken.GetVersionKey(userId),
            newVersion.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_jwt.RefreshTokenExpiresDays * 2)
            }
        );
    }

    // ─── Core Token Operations ───────────────────────────────────────────────

    public RefreshTokenResponse GenerateRefreshToken(Guid userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new RefreshTokenResponse
        {
            RefreshToken = Convert.ToBase64String(randomBytes),
            ExpiredAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiresDays)
        };
    }

    public async Task SaveRefreshTokenAsync(string refreshToken, Guid userId, DateTime expiredAt)
    {
        var currentVersion = await GetCurrentTokenVersionAsync(userId);

        var data = new RefreshTokenData
        {
            UserId = userId,
            ExpiredAt = expiredAt,
            TokenVersion = currentVersion
        };

        await _cache.SetStringAsync(
            HashedToken.GetHashedKey(refreshToken),
            JsonSerializer.Serialize(data),
            new DistributedCacheEntryOptions { AbsoluteExpiration = expiredAt }
        );
    }

    public async Task<RefreshTokenData?> GetRefreshTokenAsync(string refreshToken)
    {
        var json = await _cache.GetStringAsync(HashedToken.GetHashedKey(refreshToken));
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<RefreshTokenData>(json);
    }

    public async Task RemoveRefreshTokenAsync(string refreshToken)
        => await _cache.RemoveAsync(HashedToken.GetHashedKey(refreshToken));

    // ─── Rotate ─────────────────────────────────────────────────────────────

    public async Task<RefreshTokenResponse?> RotateRefreshTokenAsync(string oldRefreshToken)
    {
        var oldData = await GetRefreshTokenAsync(oldRefreshToken);
      
        if (oldData == null)
            return null;

        var currentVersion = await GetCurrentTokenVersionAsync(oldData.UserId);
        if (oldData.TokenVersion != currentVersion)
        {
            await RemoveRefreshTokenAsync(oldRefreshToken);
            return null;
        }

        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        var newToken = Convert.ToBase64String(randomBytes);
        
        await RemoveRefreshTokenAsync(oldRefreshToken);
        await SaveRefreshTokenAsync(newToken, oldData.UserId, oldData.ExpiredAt);

        return new RefreshTokenResponse
        {
            RefreshToken = newToken,
            ExpiredAt = oldData.ExpiredAt
        };
    }
}