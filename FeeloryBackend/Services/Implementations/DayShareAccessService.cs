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

    public async Task<bool> CanViewDayShareAsync(Guid dayShareId, Guid requesterId)
    {
        var dayShare = await _context.DayShares
            .AsNoTracking()
            .Select(d => new
            {
                d.Id,
                d.OwnerId,
                d.ShareType,
                d.DeletedAt
            })
            .FirstOrDefaultAsync(d => d.Id == dayShareId);

        if (dayShare is null || dayShare.DeletedAt is not null)
            return false;

        if (dayShare.OwnerId == requesterId)
            return true;

        return dayShare.ShareType switch
        {
            DayShareTypeConstants.Friends => await IsInDayShareViewersAsync(dayShareId, requesterId)
                                             || await IsInDayShareFeedAsync(dayShareId, requesterId),

            DayShareTypeConstants.Custom => await IsInDayShareViewersAsync(dayShareId, requesterId),

            _ => false
        };
    }

    public async Task<bool> IsDayShareOwnerAsync(Guid dayShareId, Guid userId)
    {
        return await _context.DayShares
            .AsNoTracking()
            .AnyAsync(d => d.Id == dayShareId
                        && d.OwnerId == userId
                        && d.DeletedAt == null);
    }

    // -------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------
    private async Task<bool> IsInDayShareViewersAsync(Guid dayShareId, Guid viewerId)
    {
        return await _context.DayShareViewers
            .AsNoTracking()
            .AnyAsync(v => v.DayShareId == dayShareId && v.ViewerId == viewerId);
    }

    private async Task<bool> IsInDayShareFeedAsync(Guid dayShareId, Guid viewerId)
    {
        return await _context.DayShareFeeds
            .AsNoTracking()
            .AnyAsync(f => f.DayShareId == dayShareId && f.ViewerId == viewerId);
    }
}