using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

public class PostFeedService : IPostFeedService
{
    private readonly AppDbContext _db;
    
    public PostFeedService(AppDbContext db)
    {
        _db = db;
    }
    
    // CREATE POST & ADD NEW VIEWERS
    public async Task HandleAddFeedsAsync(PostMessage message)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == message.PostId);

        if (post == null) return;

        var feeds = message.ViewerIds.Distinct()
            .Select(viewerId => new PostFeed
            {
                Id = Guid.NewGuid(),
                PostId = message.PostId,
                ViewerId = viewerId,
                PostedAt = post.CreatedAt
            });

        await _db.PostFeeds.AddRangeAsync(feeds);
        await _db.SaveChangesAsync();
    }

    // REMOVE VIEWERS
    public async Task HandleRemoveFeedsAsync(PostMessage message)
    {
        var feeds = await _db.PostFeeds
            .Where(x => x.PostId == message.PostId && message.ViewerIds.Contains(x.ViewerId))
            .ToListAsync();
        
        if (feeds.Count == 0) return;
        
        _db.PostFeeds.RemoveRange(feeds);

        await _db.SaveChangesAsync();
    }

    // DELETE POST
    public async Task HandleDeletePostAsync(PostMessage message)
    {
        var feeds = await _db.PostFeeds
            .Where(x => x.PostId == message.PostId)
            .ToListAsync();

        if (feeds.Count == 0) return;
        
        _db.PostFeeds.RemoveRange(feeds);

        await _db.SaveChangesAsync();
    }
}