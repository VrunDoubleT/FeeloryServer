using System.Text.Json;
using FeeloryBackend.Models.Entities;
using NotificationType = FeeloryBackend.Models.Enums.NotificationType;

namespace FeeloryBackend.Messaging.RabbitMQ.Consumers.Notifications.Factories;

public class NotificationFactory
{
    /// <summary>
    /// Create a notification entity
    /// </summary>
    public static Notification Create(
        Guid userId,
        Guid? actorId,
        NotificationType type,
        Guid? targetId,
        object? metadata = null)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),

            UserId = userId,

            ActorId = actorId,

            Type = type,

            TargetId = targetId,

            DataJson = metadata is null
                ? null
                : JsonSerializer.Serialize(metadata),

            IsRead = false,

            CreatedAt = DateTime.UtcNow
        };
    }
}