using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages.Auth;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FeeloryBackend.Services.Implementations;

public class HeartbeatService : IHeartbeatService
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly HeartbeatPublisher _publisher;

    public HeartbeatService(
        AppDbContext db,
        IDistributedCache cache,
        HeartbeatPublisher publisher)
    {
        _db = db;
        _cache = cache;
        _publisher = publisher;
    }

    public async Task TrackLoginAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var cacheKey = $"mission:login:{userId}:{today:yyyyMMdd}";

        var existing = await _cache.GetStringAsync(cacheKey);

        if (existing != null)
        {
            return;
        }
        
        // 2. Cache miss -> check DB
        var alreadyLoggedToday =
            await _db.UserLoginHistories
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.LoginDate == today);

        if (alreadyLoggedToday)
        {
            // Warm cache again
            await _cache.SetStringAsync(
                cacheKey,
                "1",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromDays(2)
                });

            return;
        }

        await _cache.SetStringAsync(
            cacheKey,
            "1",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromDays(2)
            });

        await _publisher.TrackLoginAsync(
            new LoginHeartbeatMessage
            {
                UserId = userId,
                LoginDate = today
            });
    }
}