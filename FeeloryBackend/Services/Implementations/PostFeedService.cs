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
    public async Task HandleAddFeedsAsync(Guid postId, IReadOnlyCollection<Guid> addedViewerIds)
    {
        var post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == postId);

        if (post == null) return;

        var feeds = addedViewerIds.Distinct()
            .Select(viewerId => new PostFeed
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                ViewerId = viewerId,
                PostedAt = post.CreatedAt
            });

        await _db.PostFeeds.AddRangeAsync(feeds);
        await _db.SaveChangesAsync();
    }

    // REMOVE VIEWERS
    public async Task HandleRemoveFeedsAsync(Guid postId, IReadOnlyCollection<Guid> removedViewerIds)
    {
        var feeds = await _db.PostFeeds
            .Where(x => x.PostId == postId && removedViewerIds.Contains(x.ViewerId))
            .ToListAsync();
        
        if (feeds.Count == 0) return;
        
        _db.PostFeeds.RemoveRange(feeds);

        await _db.SaveChangesAsync();
    }

    // DELETE POST
    public async Task HandleDeletePostAsync(Guid postId)
    {
        var feeds = await _db.PostFeeds
            .Where(x => x.PostId == postId)
            .ToListAsync();

        if (feeds.Count == 0) return;
        
        _db.PostFeeds.RemoveRange(feeds);

        await _db.SaveChangesAsync();
    }
}