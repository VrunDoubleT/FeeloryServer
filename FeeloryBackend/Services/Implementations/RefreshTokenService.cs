using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Services.Implementations;

using System.Security.Cryptography;
using System.Text.Json;
using FeeloryBackend.Models;
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
        IDistributedCache cache)
    {
        _jwt = jwtOptions.Value;
        _cache = cache;
    }

    public RefreshTokenResponse GenerateRefreshToken(Guid userId)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = Convert.ToBase64String(randomBytes);

        return new RefreshTokenResponse
        {
            RefreshToken = refreshToken,
            ExpiredAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiresDays)
        };
    }

    public async Task SaveRefreshTokenAsync(string refreshToken, Guid userId, DateTime expiredAt)
    {
        var key = $"refresh_token:{refreshToken}";

        var data = new RefreshTokenData()
        {
            UserId = userId,
            ExpiredAt = expiredAt
        };

        var json = JsonSerializer.Serialize(data);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiredAt
        };

        await _cache.SetStringAsync(key, json, options);
    }

    public async Task<RefreshTokenData?> GetRefreshTokenAsync(string refreshToken)
    {
        var key = $"refresh_token:{refreshToken}";

        var json = await _cache.GetStringAsync(key);

        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<RefreshTokenData>(json);
    }

    public async Task RemoveRefreshTokenAsync(string refreshToken)
    {
        var key = $"refresh_token:{refreshToken}";
        await _cache.RemoveAsync(key);
    }

    // Rotate refresh token (Absolute Expiration)
    public async Task<RefreshTokenResponse?> RotateRefreshTokenAsync(string oldRefreshToken)
    {
        var oldData = await GetRefreshTokenAsync(oldRefreshToken);

        if (oldData == null)
            return null;

        if (oldData.ExpiredAt < DateTime.UtcNow)
        {
            await RemoveRefreshTokenAsync(oldRefreshToken);
            return null;
        }

        // Create new refresh token
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var newToken = Convert.ToBase64String(randomBytes);

        var response = new RefreshTokenResponse
        {
            RefreshToken = newToken,
            ExpiredAt = oldData.ExpiredAt
        };

        // Delete old token
        await RemoveRefreshTokenAsync(oldRefreshToken);

        // Save new token with same expiration time
        await SaveRefreshTokenAsync(newToken, oldData.UserId, oldData.ExpiredAt);

        return response;
    }
}