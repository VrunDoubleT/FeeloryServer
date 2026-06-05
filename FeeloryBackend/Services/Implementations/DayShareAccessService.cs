using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class DayShareAccessService : IDayShareAccessService
{
    private readonly AppDbContext _context;

    public DayShareAccessService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Check if a user can view a DayShare.
    /// RULE:
    /// - Owner always allowed
    /// - Otherwise must exist in DayShareFeeds
    /// </summary>
    public async Task<bool> CanViewDayShareAsync(Guid dayShareId, Guid requesterId)
    {
        // 1. Check owner OR feed in ONE query
        return await _context.DayShares
            .AsNoTracking()
            .AnyAsync(d =>
                d.Id == dayShareId &&
                d.DeletedAt == null &&
                (
                    d.OwnerId == requesterId ||
                    _context.DayShareFeeds.Any(f =>
                        f.DayShareId == dayShareId &&
                        f.ViewerId == requesterId)
                )
            );
    }

    /// <summary>
    /// Check if user is owner of DayShare
    /// </summary>
    public async Task<bool> IsDayShareOwnerAsync(Guid dayShareId, Guid userId)
    {
        return await _context.DayShares
            .AsNoTracking()
            .AnyAsync(d =>
                d.Id == dayShareId &&
                d.OwnerId == userId &&
                d.DeletedAt == null);
    }
    
    /// <summary>
    /// Checks if the specified post is included in any DayShare that the requester has access to.
    /// Access is determined via DayShareFeed visibility (not direct ownership).
    /// </summary>
    public async Task<bool> IsPostInAnyVisibleDayShareAsync(Guid postId, Guid requesterId)
    {
        return await _context.DaySharePosts
            .AsNoTracking()
            .AnyAsync(dsp =>
                dsp.PostId == postId &&
                dsp.DayShare.DayShareFeeds.Any(f =>
                    f.ViewerId == requesterId));
    }
}