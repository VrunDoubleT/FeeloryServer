using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;

namespace FeeloryBackend.Services.Interfaces;

public interface IPostFeedService
{
    // Create post & add new viewers
    Task HandleAddFeedsAsync(Guid postId, IReadOnlyCollection<Guid> addedViewerIds);
    
    // Remove old viewers
    Task HandleRemoveFeedsAsync(Guid postId, IReadOnlyCollection<Guid> removedViewerIds);
    
    // Delete post
    Task HandleDeletePostAsync(Guid postId);
}