using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Services.Implementations;

public static class DayShareFeedService
{
    // Dùng chung cho CREATED và ADDED
    public static async Task HandleAddFeedsAsync(
        AppDbContext db,
        DayShareFeedMessage message)
    {
        foreach (var viewerId in message.ViewerIds.Distinct())
        {
            bool exists = await db.DayShareFeeds.AnyAsync(x =>
                x.DayShareId == message.DayShareId &&
                x.ViewerId == viewerId);

            if (exists) continue;

            db.DayShareFeeds.Add(new DayShareFeed
            {
                Id         = Guid.NewGuid(),
                DayShareId = message.DayShareId,
                ViewerId   = viewerId,
                PostedAt   = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    public static async Task HandleRemovedAsync(
        AppDbContext db,
        DayShareFeedMessage message)
    {
        var toRemove = await db.DayShareFeeds
            .Where(x =>
                x.DayShareId == message.DayShareId &&
                message.ViewerIds.Contains(x.ViewerId))
            .ToListAsync();

        db.DayShareFeeds.RemoveRange(toRemove);

        await db.SaveChangesAsync();
    }

    public static async Task HandleDeletedAsync(
        AppDbContext db,
        DayShareFeedMessage message)
    {
        var feeds = await db.DayShareFeeds
            .Where(x => x.DayShareId == message.DayShareId)
            .ToListAsync();

        db.DayShareFeeds.RemoveRange(feeds);

        await db.SaveChangesAsync();
    }
}