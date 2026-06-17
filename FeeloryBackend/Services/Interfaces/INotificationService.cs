using System;
using System.Threading.Tasks;
using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.Notification;

namespace FeeloryBackend.Services.Interfaces;

public interface INotificationService
{
    // Get notifications of user
    Task<Result<NotificationPaginationResponse>> GetByUserAsync(Guid userId, CursorPaginationRequest request);

    // Mark notification as read
    Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId);

    // Mark all as read
    Task<Result<int>> MarkAllAsReadAsync(Guid userId);
}