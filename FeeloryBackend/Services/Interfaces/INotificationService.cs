using FeeloryBackend.Models.DTOs.Notification;

namespace FeeloryBackend.Services.Interfaces;

public interface INotificationService
{
    // Get notifications of user
    Task<List<NotificationDto>> GetByUserAsync(Guid userId);

    // Mark notification as read
    Task MarkAsReadAsync(Guid notificationId);

    // Mark all as read
    Task MarkAllAsReadAsync(Guid userId);
}