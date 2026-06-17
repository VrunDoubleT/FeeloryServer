using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;

namespace FeeloryBackend.Services.Interfaces;

public interface IDayShareFeedService
{
    Task HandleAddFeedsAsync(Guid dayShareId, IReadOnlyCollection<Guid> addedViewerIds);

    Task HandleRemovedAsync(Guid dayShareId, IReadOnlyCollection<Guid> removedViewerIds);

    Task HandleDeletedAsync(Guid dayShareId);
}