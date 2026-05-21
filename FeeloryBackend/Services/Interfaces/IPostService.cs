using FeeloryBackend.Models.DTOs.Post;

namespace FeeloryBackend.Services.Interfaces;

public interface IPostService
{
    // Create diary post
    Task<Guid> CreateAsync(Guid userId, CreatePostRequestDto request);

    // Update post
    Task UpdateAsync(Guid userId, Guid postId, UpdatePostRequestDto request);

    // Delete post
    Task DeleteAsync(Guid userId, Guid postId);

    // Get posts by user
    Task<List<PostDto>> GetByUserAsync(Guid userId);
}