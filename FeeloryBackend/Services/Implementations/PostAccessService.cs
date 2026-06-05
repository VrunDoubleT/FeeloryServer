using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class PostAccessService : IPostAccessService
{
    private readonly AppDbContext _context;
    private readonly IDayShareAccessService _dayShareAccessService;

    public PostAccessService(
        AppDbContext context,
        IDayShareAccessService dayShareAccessService)
    {
        _context = context;
        _dayShareAccessService = dayShareAccessService;
    }

    public async Task<bool> CanViewPostAsync(Guid postId, Guid requesterId)
    {
        // Fetch minimal post data
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

        // Not found / deleted
        if (post is null || post.DeletedAt != null)
            return false;

        // Owner always has full access
        if (post.UserId == requesterId)
            return true;

        // --------------------------------------------
        // 1. Normal visibility rules (NON-DAYSHARE PATH)
        // --------------------------------------------
        if (post.Privacy != PostPrivacyConstants.Private)
        {
            if (await IsInPostFeedAsync(postId, requesterId))
                return true;
        }

        // --------------------------------------------
        // 2. IMPORTANT OVERRIDE RULE (DAYSHARE ACCESS)
        // --------------------------------------------
        // Even PRIVATE posts can be interacted with
        // if they are included in a DayShare visible to user
        if (await _dayShareAccessService.IsPostInAnyVisibleDayShareAsync(postId, requesterId))
            return true;

        return false;
    }

    public async Task<bool> IsPostOwnerAsync(Guid postId, Guid userId)
    {
        // Check whether the given user is the owner of the post
        return await _context.Posts
            .AsNoTracking()
            .AnyAsync(p => p.Id == postId
                           && p.UserId == userId
                           && p.DeletedAt == null);
    }

    // -------------------------------------------------------
    // Private helper methods
    // -------------------------------------------------------

    /// <summary>
    /// Checks whether the post is explicitly included in the PostFeed
    /// (direct sharing mechanism).
    /// </summary>
    private async Task<bool> IsInPostFeedAsync(Guid postId, Guid viewerId)
    {
        return await _context.PostFeeds
            .AsNoTracking()
            .AnyAsync(pf =>
                pf.PostId == postId &&
                pf.ViewerId == viewerId);
    }
}