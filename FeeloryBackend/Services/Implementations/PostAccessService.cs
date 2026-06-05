using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class PostAccessService : IPostAccessService
{
    private readonly AppDbContext _context;

    public PostAccessService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanViewPostAsync(Guid postId, Guid requesterId)
    {
        var post = await _context.Posts
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.UserId,
                p.Privacy,
                p.DeletedAt
            })
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post is null || post.DeletedAt != null)
            return false;

        if (post.UserId == requesterId)
            return true;

        if (post.Privacy == PostPrivacyConstants.Private)
            return false;

        return await IsInPostFeedAsync(postId, requesterId);
    }

    public async Task<bool> IsPostOwnerAsync(Guid postId, Guid userId)
    {
        return await _context.Posts
            .AsNoTracking()
            .AnyAsync(p => p.Id == postId
                        && p.UserId == userId
                        && p.DeletedAt == null);
    }

    // -------------------------------------------------------
    // Private helper methods
    // -------------------------------------------------------
    private async Task<bool> IsInPostFeedAsync(Guid postId, Guid viewerId)
    {
        return await _context.PostFeeds
            .AsNoTracking()
            .AnyAsync(pf => pf.PostId == postId && pf.ViewerId == viewerId);
    }
}