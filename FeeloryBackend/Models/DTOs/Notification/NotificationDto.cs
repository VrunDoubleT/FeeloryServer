using System;
using System.Collections.Generic;

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

public class NotificationListDto
{
    public int UnreadCount { get; set; }
    public IEnumerable<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
    public string? NextCursor { get; set; }
    public bool HasNextPage { get; set; }
}