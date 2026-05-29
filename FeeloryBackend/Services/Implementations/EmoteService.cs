using System.Text.Json;
using FeeloryBackend.Commons;
using FeeloryBackend.Data;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FeeloryBackend.Services.Implementations;

public class EmoteService : IEmoteService
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;

    private const string CacheKey = "all_emotes_grouped_v2";

    public EmoteService(AppDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<Result<Dictionary<string, List<EmoteDto>>>> GetAllGroupedAsync()
    {
        // Redis
        var cachedData = await _cache.GetStringAsync(CacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var grouped = JsonSerializer.Deserialize<Dictionary<string, List<EmoteDto>>>(cachedData);
            if (grouped != null) return Result<Dictionary<string, List<EmoteDto>>>.Ok(grouped);
        }

        // 2. Query DB
        var packages = await _db.EmotePackages
            .AsNoTracking()
            .Include(p => p.Items)
            .ThenInclude(i => i.Emote)
            .ToListAsync();

        var groupedResult = packages.ToDictionary(
            p => p.Name,
            p => p.Items.Select(i => new EmoteDto
            {
                Id = i.Emote.Id,
                Name = i.Emote.Name,
                ImageUrl = i.Emote.ImageUrl
            }).ToList()
        );

        // Save to Redis 1 hour
        var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };
        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(groupedResult), cacheOptions);

        return Result<Dictionary<string, List<EmoteDto>>>.Ok(groupedResult);
    }

    public async Task<Result<EmoteDto>> GetByIdAsync(Guid id)
    {
        var emote = await _db.Emotes
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EmoteDto { Id = e.Id, Name = e.Name, ImageUrl = e.ImageUrl })
            .FirstOrDefaultAsync();

        if (emote is null) return Result<EmoteDto>.Fail("Emote does not exist.");
        return Result<EmoteDto>.Ok(emote);
    }

    public async Task<Result<List<UserEmoteDto>>> GetUserEmotesAsync(Guid userId)
    {
        var defaultEmotes = await _db.EmotePackages
            .AsNoTracking()
            .Where(p => p.IsDefault)
            .SelectMany(p => p.Items.Select(i => new UserEmoteDto
            {
                Emote = new EmoteDto { Id = i.Emote.Id, Name = i.Emote.Name, ImageUrl = i.Emote.ImageUrl },
                UnlockedAt = null
            }))
            .ToListAsync();

        var unlockedEmotes = await _db.UserPackages
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .SelectMany(up => up.Package.Items.Select(i => new UserEmoteDto
            {
                Emote = new EmoteDto { Id = i.Emote.Id, Name = i.Emote.Name, ImageUrl = i.Emote.ImageUrl },
                UnlockedAt = up.UnlockedAt
            }))
            .ToListAsync();

        var result = defaultEmotes.Concat(unlockedEmotes)
            .GroupBy(u => u.Emote.Id)
            .Select(g => g.First())
            .ToList();

        return Result<List<UserEmoteDto>>.Ok(result);
    }

    public async Task<Result<List<EmoteDto>>> GetRecentEmotesAsync(Guid userId, int limit)
    {
        int take = limit > 0 ? limit : 10;

        var recentEmotes = await _db.Emotes
            .AsNoTracking()
            // Get the emoticons that this user has used before.
            .Where(e => e.Reactions.Any(r => r.UserId == userId))
            // Extract the emote along with the user's most recent usage time.
            .Select(e => new
            {
                Emote = e,
                LastUsed = e.Reactions.Where(r => r.UserId == userId).Max(r => r.CreatedAt)
            })
            // Sort in descending order
            .OrderByDescending(x => x.LastUsed)
            // Top N
            .Take(take)
            .Select(x => new EmoteDto
            {
                Id = x.Emote.Id,
                Name = x.Emote.Name,
                ImageUrl = x.Emote.ImageUrl
            })
            .ToListAsync();

        return Result<List<EmoteDto>>.Ok(recentEmotes);
    }
}