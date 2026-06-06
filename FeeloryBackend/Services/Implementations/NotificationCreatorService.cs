using FeeloryBackend.Data;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
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

        await _context.SaveChangesAsync(
            cancellationToken);
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

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}