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

        // Post does not exist or has been deleted
        if (post is null || post.DeletedAt is not null)
            return false;

        // Post owner always has access
        if (post.UserId == requesterId)
            return true;

        return post.Privacy switch
        {
            PostPrivacyConstants.Public => true,

            PostPrivacyConstants.Private => false,

            // Custom visibility: check PostViewers or PostFeeds
            PostPrivacyConstants.Custom => await IsInPostViewersAsync(postId, requesterId)
                                         || await IsInPostFeedAsync(postId, requesterId),

            _ => false
        };
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
    private async Task<bool> IsInPostViewersAsync(Guid postId, Guid viewerId)
    {
        return await _context.PostViewers
            .AsNoTracking()
            .AnyAsync(pv => pv.PostId == postId && pv.ViewerId == viewerId);
    }

    private async Task<bool> IsInPostFeedAsync(Guid postId, Guid viewerId)
    {
        return await _context.PostFeeds
            .AsNoTracking()
            .AnyAsync(pf => pf.PostId == postId && pf.ViewerId == viewerId);
    }
}