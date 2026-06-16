using System;
using System.Threading.Tasks;
using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.Notification;

namespace FeeloryBackend.Services.Interfaces;

public interface INotificationService
{
    Task<Result<NotificationListDto>> GetByUserAsync(Guid userId, CursorPaginationRequest request);

    Task<Result> MarkAsReadAsync(Guid userId, Guid notificationId);

    Task<Result<int>> MarkAllAsReadAsync(Guid userId);
}