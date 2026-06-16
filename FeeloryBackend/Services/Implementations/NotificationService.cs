using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FeeloryBackend.Commons;
using FeeloryBackend.Data;
using FeeloryBackend.Helpers;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.Notification;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<NotificationListDto>> GetByUserAsync(Guid userId, CursorPaginationRequest request)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Include(n => n.Actor)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .AsQueryable();

        // Cursor Pagination
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, id) = CursorHelper.Decode(request.Cursor);
            query = query.Where(n =>
                n.CreatedAt < createdAt ||
                (n.CreatedAt == createdAt && n.Id.CompareTo(id) < 0));
        }

        var notifications = await query
            .Take(request.PageSize + 1)
            .ToListAsync();

        bool hasNextPage = notifications.Count > request.PageSize;
        notifications = notifications.Take(request.PageSize).ToList();

        string? nextCursor = null;
        if (hasNextPage)
        {
            var lastItem = notifications.Last();
            nextCursor = CursorHelper.Encode(lastItem.CreatedAt, lastItem.Id);
        }

        // =========================================================
        // GET ALL ATTACHED INFORMATION READY
        // =========================================================

        var missionIds = notifications.Where(n => n.Type == NotificationType.MissionCompleted && n.TargetId.HasValue).Select(n => n.TargetId!.Value).Distinct().ToList();
        var packageIds = notifications.Where(n => n.Type == NotificationType.GiftReceived && n.TargetId.HasValue).Select(n => n.TargetId!.Value).Distinct().ToList();
        var postIds = notifications.Where(n => (n.Type == NotificationType.PostCreated || n.Type == NotificationType.PostReactionAdded) && n.TargetId.HasValue).Select(n => n.TargetId!.Value).Distinct().ToList();
        var dayShareIds = notifications.Where(n => n.Type == NotificationType.DayShareCreated && n.TargetId.HasValue).Select(n => n.TargetId!.Value).Distinct().ToList();

        var emoteIds = new List<Guid>();
        foreach (var n in notifications.Where(x => x.Type == NotificationType.PostReactionAdded && !string.IsNullOrWhiteSpace(x.DataJson)))
        {
            try
            {
                var doc = JsonDocument.Parse(n.DataJson!);
                if (doc.RootElement.TryGetProperty("EmoteId", out var prop) && Guid.TryParse(prop.GetString(), out var eId))
                    emoteIds.Add(eId);
            }
            catch {}
        }
        emoteIds = emoteIds.Distinct().ToList();

        var missionsDict = await _db.Missions.Include(m => m.Rewards).ThenInclude(r => r.Package).Where(m => missionIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id);
        var packagesDict = await _db.EmotePackages.Where(p => packageIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        var postsDict = await _db.Posts.Where(p => postIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        var daySharesDict = await _db.DayShares.Where(d => dayShareIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id);
        var emotesDict = await _db.Emotes.Where(e => emoteIds.Contains(e.Id)).ToDictionaryAsync(e => e.Id);

        // Map DTO
        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = GetMappedTypeString(n.Type),
            Title = GenerateNotificationTitle(n.Type),
            Message = GenerateNotificationMessage(n),
            TargetId = n.TargetId,
            Data = GenerateNotificationData(n, missionsDict, packagesDict, postsDict, daySharesDict, emotesDict),
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        int unreadCount = await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        var responseData = new NotificationListDto
        {
            UnreadCount = unreadCount,
            Notifications = dtos,
            NextCursor = nextCursor,
            HasNextPage = hasNextPage
        };

        return Result<NotificationListDto>.Ok(responseData);
    }

    public async Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification is null)
            return Result.Fail("Notification not found.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return Result.Ok();
    }

    public async Task<Result<int>> MarkAllAsReadAsync(Guid userId)
    {
        var updatedCount = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTime.UtcNow));

        return Result<int>.Ok(updatedCount);
    }

    // ==========================================
    // HELPER
    // ==========================================
    private string TruncateText(string? text, int maxLength = 50)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
    }

    // ==========================================
    // HELPER:Object Data
    // ==========================================
    private object? GenerateNotificationData(
        Notification notification,
        Dictionary<Guid, Mission> missionsDict,
        Dictionary<Guid, EmotePackage> packagesDict,
        Dictionary<Guid, Post> postsDict,
        Dictionary<Guid, DayShare> daySharesDict,
        Dictionary<Guid, Emote> emotesDict)
    {
        JsonDocument? dbData = null;
        if (!string.IsNullOrEmpty(notification.DataJson))
        {
            try { dbData = JsonDocument.Parse(notification.DataJson); }
            catch {}
        }

        var actorInfo = notification.Actor != null ? new
        {
            id = notification.Actor.Id,
            displayName = notification.Actor.DisplayName,
            avatarUrl = notification.Actor.AvatarUrl
        } : null;

        Guid? parsedEmoteId = null;
        if (dbData?.RootElement.TryGetProperty("EmoteId", out var emoteProp) == true && Guid.TryParse(emoteProp.GetString(), out var eId))
        {
            parsedEmoteId = eId;
        }

        return notification.Type switch
        {
            NotificationType.FriendRequestReceived or NotificationType.FriendRequestAccepted => new
            {
                user = actorInfo
            },

            NotificationType.PostReactionAdded => new
            {
                user = actorInfo,
                post = notification.TargetId.HasValue && postsDict.TryGetValue(notification.TargetId.Value, out var postReacted) ? new
                {
                    id = postReacted.Id,
                    description = postReacted.Description,
                    imageUrl = postReacted.ImageUrl,
                    privacy = postReacted.Privacy,
                    createdAt = postReacted.CreatedAt
                } : null,
                emote = parsedEmoteId.HasValue && emotesDict.TryGetValue(parsedEmoteId.Value, out var emote) ? new
                {
                    id = emote.Id,
                    name = emote.Name,
                    imageUrl = emote.ImageUrl
                } : null
            },

            NotificationType.PostCreated => new
            {
                user = actorInfo,
                post = notification.TargetId.HasValue && postsDict.TryGetValue(notification.TargetId.Value, out var postCreated) ? new
                {
                    id = postCreated.Id,
                    description = postCreated.Description,
                    imageUrl = postCreated.ImageUrl,
                    privacy = postCreated.Privacy,
                    createdAt = postCreated.CreatedAt
                } : null
            },

            NotificationType.DayShareCreated => new
            {
                user = actorInfo,
                dayShare = notification.TargetId.HasValue && daySharesDict.TryGetValue(notification.TargetId.Value, out var dayShare) ? new
                {
                    id = dayShare.Id,
                    description = dayShare.Description,
                    shareType = dayShare.ShareType,
                    createdAt = dayShare.SharedDate
                } : null
            },

            NotificationType.MissionCompleted => new
            {
                mission = notification.TargetId.HasValue && missionsDict.TryGetValue(notification.TargetId.Value, out var mission) ? new
                {
                    id = mission.Id,
                    name = mission.Name,
                    description = mission.Description,
                    targetValue = mission.TargetValue,
                    rewards = mission.Rewards.Select(r => new
                    {
                        packageId = r.PackageId,
                        packageName = r.Package.Name,
                        coverUrl = r.Package.CoverUrl
                    }).ToList()
                } : null
            },

            NotificationType.GiftReceived => new
            {
                package = notification.TargetId.HasValue && packagesDict.TryGetValue(notification.TargetId.Value, out var package) ? new
                {
                    id = package.Id,
                    name = package.Name,
                    description = package.Description,
                    coverUrl = package.CoverUrl
                } : null,
                extra = dbData?.RootElement
            },

            _ => new { user = actorInfo, extra = dbData?.RootElement }
        };
    }

    // ==========================================
    // HELPER: Mapping Title & Message
    // ==========================================
    private string GetMappedTypeString(NotificationType type) => type switch
    {
        NotificationType.PostReactionAdded => "reaction",
        NotificationType.DayShareCreated => "dayshare",
        NotificationType.FriendRequestReceived => "friend_request_received",
        NotificationType.FriendRequestAccepted => "friend_request_accepted",
        NotificationType.MissionCompleted => "mission_completed",
        NotificationType.PostCreated => "post",
        NotificationType.GiftReceived => "gift",
        _ => "system"
    };

    private string GenerateNotificationTitle(NotificationType type) => type switch
    {
        NotificationType.PostCreated => "New post",
        NotificationType.DayShareCreated => "New moment",
        NotificationType.PostReactionAdded => "Post reaction",
        NotificationType.FriendRequestReceived => "Friend request",
        NotificationType.FriendRequestAccepted => "New friend",
        NotificationType.MissionCompleted => "Mission completed",
        NotificationType.GiftReceived => "System gift",
        _ => "Notification"
    };

    private string GenerateNotificationMessage(Notification notification)
    {
        string actorName = notification.Actor?.DisplayName ?? "System";

        return notification.Type switch
        {
            NotificationType.PostCreated => $"{actorName} just published a new post.",
            NotificationType.DayShareCreated => $"{actorName} just shared a new moment.",
            NotificationType.PostReactionAdded => $"{actorName} reacted to your post.",
            NotificationType.FriendRequestReceived => $"{actorName} sent you a friend request.",
            NotificationType.FriendRequestAccepted => $"{actorName} accepted your friend request.",
            NotificationType.MissionCompleted => "Congratulations! You have completed a mission.",
            NotificationType.GiftReceived => "You have just received a gift from the system.",
            _ => "You have a new notification."
        };
    }
}