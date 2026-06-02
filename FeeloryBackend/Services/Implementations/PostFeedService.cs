using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

public class PostFeedService : IPostFeedService
{
    // CREATE POST & ADD NEW VIEWERS
    public async Task HandleAddFeedsAsync(AppDbContext db, PostMessage message)
    {
        var post = await db.Posts.FirstOrDefaultAsync(x => x.Id == message.PostId);

        if (post == null) return;

        var feeds = message.ViewerIds.Distinct()
            .Select(viewerId => new PostFeed
            {
                Id = Guid.NewGuid(),
                PostId = message.PostId,
                ViewerId = viewerId,
                PostedAt = post.CreatedAt
            });

        await db.PostFeeds.AddRangeAsync(feeds);
        await db.SaveChangesAsync();
    }

    // REMOVE VIEWERS
    public async Task HandleRemoveFeedsAsync(AppDbContext db, PostMessage message)
    {
        var feeds = await db.PostFeeds
            .Where(x => x.PostId == message.PostId && message.ViewerIds.Contains(x.ViewerId))
            .ToListAsync();
        
        if (feeds.Count == 0) return;
        
        db.PostFeeds.RemoveRange(feeds);

        await db.SaveChangesAsync();
    }

    // DELETE POST
    public async Task HandleDeletePostAsync(AppDbContext db, PostMessage message)
    {
        var feeds = await db.PostFeeds
            .Where(x => x.PostId == message.PostId)
            .ToListAsync();

        if (feeds.Count == 0) return;
        
        db.PostFeeds.RemoveRange(feeds);

        await db.SaveChangesAsync();
    }
}