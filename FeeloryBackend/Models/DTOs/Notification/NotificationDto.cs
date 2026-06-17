using System;
using System.Collections.Generic;
using FeeloryBackend.Responses;

namespace FeeloryBackend.Models.DTOs.Notification;

public class NotificationDto
{
    public Guid Id { get; set; }

    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public Guid? TargetId { get; set; }

    public object? Data { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class NotificationPaginationResponse : CursorPaginationResponse<NotificationDto>
{
    public int UnreadCount { get; set; }

    public NotificationPaginationResponse(
        IEnumerable<NotificationDto> data,
        string? nextCursor,
        bool hasNextPage,
        int unreadCount,
        string message = "Retrieved notifications successfully")
        : base(data, nextCursor, hasNextPage, message)
    {
        UnreadCount = unreadCount;
    }
}