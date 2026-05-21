using FeeloryBackend.Models.DTOs.Post;

namespace FeeloryBackend.Services.Interfaces;

public interface IFeedService
{
    // Get feed for user (home timeline)
    Task<List<PostDto>> GetFeedAsync(Guid userId, int page, int pageSize);

    // Generate feed (internal system)
    Task GenerateFeedAsync(Guid userId, Guid postId);

    // Mark post as viewed in feed
    Task MarkViewedAsync(Guid userId, Guid postId);
}