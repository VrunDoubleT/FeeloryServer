using System;
using System.Linq;
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

    public async Task<Result<NotificationPaginationResponse>> GetByUserAsync(Guid userId, CursorPaginationRequest request)
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

        // Map DTO
        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = GetMappedTypeString(n.Type),
            Title = GenerateNotificationTitle(n.Type),
            Message = GenerateNotificationMessage(n),
            TargetId = n.TargetId,
            DataJson = n.DataJson,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        // Count the total number of unread notifications
        int unreadCount = await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        // class Response
        var responseData = new NotificationPaginationResponse(
            dtos,
            nextCursor,
            hasNextPage,
            unreadCount
        );

        return Result<NotificationPaginationResponse>.Ok(responseData);
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
    // HELPER: Type và Message
    // ==========================================
    private string GetMappedTypeString(NotificationType type) => type switch
    {
        NotificationType.PostReactionAdded => "reaction",
        NotificationType.DayShareCreated => "dayshare",
        NotificationType.FriendRequestReceived => "friend_request",
        NotificationType.FriendRequestAccepted => "friend_request",
        NotificationType.MissionCompleted => "task_complete",
        NotificationType.PostCreated => "post",
        NotificationType.GiftReceived => "gift",
        _ => "system"
    };

    private string GenerateNotificationMessage(Notification notification)
    {
        string actorName = notification.Actor?.DisplayName ?? "System";

        return notification.Type switch
        {
            NotificationType.PostCreated => $"{actorName} just posted a new article.",
            NotificationType.DayShareCreated => $"{actorName} just shared a moment.",
            NotificationType.PostReactionAdded => $"{actorName} They have expressed their feelings about your post.",
            NotificationType.FriendRequestReceived => $"{actorName} They have expressed their feelings about your post.",
            NotificationType.FriendRequestAccepted => $"{actorName} I have accepted your friend request.",
            NotificationType.MissionCompleted => "Congratulations! You have completed a task.",
            NotificationType.GiftReceived => "You have just received a gift from the system.",
            _ => "You have a new notification."
        };
    }

    private string GenerateNotificationTitle(NotificationType type) => type switch
    {
        NotificationType.PostCreated => "New article",
        NotificationType.DayShareCreated => "New moment",
        NotificationType.PostReactionAdded => "Article interaction",
        NotificationType.FriendRequestReceived => "Friend invitation",
        NotificationType.FriendRequestAccepted => "New friends",
        NotificationType.MissionCompleted => "Mission completed",
        NotificationType.GiftReceived => "System gift",
        _ => "Notification"
    };
}