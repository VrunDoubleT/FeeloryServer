using FeeloryBackend.Models.DTOs.Post;

namespace FeeloryBackend.Services.Interfaces;

public interface IPostService
{
    // Create diary post
    Task<Guid> CreateAsync(Guid userId, CreatePostRequestDto request);

    // Update post
    Task<bool> UpdateAsync(Guid userId, Guid postId, UpdatePostRequestDto request);

    // Delete post
    Task<bool> DeleteAsync(Guid userId, Guid postId);

    // Get posts by user
    Task<GetMyPostsResponseDto> GetMyPostsAsync(Guid userId, GetMyPostsRequestDto request);
    
    // Get posts by id
    Task<PostDetailDto?> GetByIdAsync(Guid currentUserId, Guid postId);
    
    // Get friend post feed
    Task<GetFriendFeedResponseDto> GetFriendFeedAsync(Guid currentUserId, GetFriendFeedRequestDto request);
}