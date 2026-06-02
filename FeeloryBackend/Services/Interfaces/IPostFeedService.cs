using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;

namespace FeeloryBackend.Services.Interfaces;

public interface IPostFeedService
{
    // Create post & add new viewers
    Task HandleAddFeedsAsync(AppDbContext db, PostMessage message);
    
    // Remove old viewers
    Task HandleRemoveFeedsAsync(AppDbContext db, PostMessage message);
    
    // Delete post
    Task HandleDeletePostAsync(AppDbContext db, PostMessage message);
}