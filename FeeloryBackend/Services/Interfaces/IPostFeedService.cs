using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;

namespace FeeloryBackend.Services.Interfaces;

public interface IPostFeedService
{
    // Create post & add new viewers
    Task HandleAddFeedsAsync(PostMessage message);
    
    // Remove old viewers
    Task HandleRemoveFeedsAsync(PostMessage message);
    
    // Delete post
    Task HandleDeletePostAsync(PostMessage message);
}