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

    public async Task HandleAddFeedsAsync(
        DayShareFeedMessage message)
    {
        foreach (var viewerId in message.ViewerIds.Distinct())
        {
            bool exists = await _db.DayShareFeeds.AnyAsync(x =>
                x.DayShareId == message.DayShareId &&
                x.ViewerId == viewerId);

            if (exists)
                continue;

            _db.DayShareFeeds.Add(new DayShareFeed
            {
                Id = Guid.NewGuid(),
                DayShareId = message.DayShareId,
                ViewerId = viewerId,
                PostedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task HandleRemovedAsync(
        DayShareFeedMessage message)
    {
        var feeds = await _db.DayShareFeeds
            .Where(x =>
                x.DayShareId == message.DayShareId &&
                message.ViewerIds.Contains(x.ViewerId))
            .ToListAsync();

        _db.DayShareFeeds.RemoveRange(feeds);

        await _db.SaveChangesAsync();
    }

    public async Task HandleDeletedAsync(
        DayShareFeedMessage message)
    {
        var feeds = await _db.DayShareFeeds
            .Where(x =>
                x.DayShareId == message.DayShareId)
            .ToListAsync();

        _db.DayShareFeeds.RemoveRange(feeds);

        await _db.SaveChangesAsync();
    }
}
