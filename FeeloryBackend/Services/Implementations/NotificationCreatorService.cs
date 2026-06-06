using System.Text.Json;
using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Consumers.Notifications.Factories;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Services.Implementations;

public class NotificationCreatorService : INotificationCreatorService
{
    private readonly AppDbContext _context;

    public NotificationCreatorService(
        AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Create a single notification
    /// </summary>
    public async Task CreateAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        await _context.Notifications.AddAsync(
            notification,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Create multiple notifications
    /// </summary>
    public async Task CreateRangeAsync(
        IEnumerable<Notification> notifications,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notifications);

        var notificationList = notifications.ToList();

        if (notificationList.Count == 0)
        {
            return;
        }

        await _context.Notifications.AddRangeAsync(
            notificationList,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task CreateOrUpdateReactionAsync(
        Guid ownerId,
        Guid reactorId,
        Guid postId,
        Guid emoteId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(
                x =>
                    x.UserId == ownerId &&
                    x.ActorId == reactorId &&
                    x.TargetId == postId &&
                    x.Type == NotificationType.PostReactionAdded,
                cancellationToken);

        if (notification is not null)
        {
            // Update emote metadata
            notification.DataJson = JsonSerializer.Serialize(
                new
                {
                    EmoteId = emoteId
                });
            
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        notification = NotificationFactory.Create(
            userId: ownerId,
            actorId: reactorId,
            type: NotificationType.PostReactionAdded,
            targetId: postId,
            metadata: new
            {
                EmoteId = emoteId
            });

        await _context.Notifications.AddAsync(
            notification,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}