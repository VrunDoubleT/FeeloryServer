using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Services.Implementations;

public class DayShareFeedService : IDayShareFeedService
{
    private readonly AppDbContext _db;

    public DayShareFeedService(
        AppDbContext db)
    {
        _db = db;
    }

    public async Task HandleAddFeedsAsync(Guid dayShareId, IReadOnlyCollection<Guid> addedViewerIds)
    {
        foreach (var viewerId in addedViewerIds.Distinct())
        {
            bool exists = await _db.DayShareFeeds.AnyAsync(x =>
                x.DayShareId == dayShareId &&
                x.ViewerId == viewerId);

            if (exists)
            {
                continue;
            }

            _db.DayShareFeeds.Add(new DayShareFeed
            {
                Id = Guid.NewGuid(),
                DayShareId = dayShareId,
                ViewerId = viewerId,
                PostedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task HandleRemovedAsync(Guid dayShareId, IReadOnlyCollection<Guid> removedViewerIds)
    {
        var feeds = await _db.DayShareFeeds
            .Where(x =>
                x.DayShareId == dayShareId &&
                removedViewerIds.Contains(x.ViewerId))
            .ToListAsync();

        _db.DayShareFeeds.RemoveRange(feeds);

        await _db.SaveChangesAsync();
    }

    public async Task HandleDeletedAsync(Guid dayShareId)
    {
        var feeds = await _db.DayShareFeeds
            .Where(x =>
                x.DayShareId == dayShareId)
            .ToListAsync();

        _db.DayShareFeeds.RemoveRange(feeds);

        await _db.SaveChangesAsync();
    }
}
