using FeeloryBackend.Models.Entities;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Services.Interfaces;

public interface INotificationCreatorService
{
    /// <summary>
    /// Create a single notification
    /// </summary>
    Task CreateAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create multiple notifications
    /// </summary>
    Task CreateRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);

    Task CreateOrUpdateReactionAsync(
        Guid ownerId,
        Guid reactorId,
        Guid postId,
        Guid emoteId,
        CancellationToken cancellationToken = default);
}