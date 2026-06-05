using FeeloryBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Extensions;

public static class EmoteExtensions
{
    public static async Task<bool> CanUseEmoteAsync(this AppDbContext db, Guid userId, Guid emoteId)
    {
        return await db.Emotes.AnyAsync(
            e => e.Id == emoteId && e.EmotePackageItems.Any(item => 
                item.Package.IsDefault || db.UserPackages.Any(up => 
                    up.UserId == userId &&
                    up.PackageId == item.PackageId)
                ));
    }
}