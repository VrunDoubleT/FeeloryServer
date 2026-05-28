using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Post;

namespace FeeloryBackend.Services.Interfaces;

public interface IPostService
{
    // Create diary post
    Task<Result<Guid>> CreateAsync(Guid userId, CreatePostRequestDto request);

    // Update post
    Task<Result> UpdateAsync(Guid userId, Guid postId, UpdatePostRequestDto request);

    // Delete post
    Task<Result> DeleteAsync(Guid userId, Guid postId);

    // Get posts by user
    Task<GetMyPostsResponseDto> GetMyPostsAsync(Guid userId, GetMyPostsRequestDto request);
    
    // Get posts by id
    Task<PostDetailDto?> GetByIdAsync(Guid currentUserId, Guid postId);
    
    // Get friend post feed
    Task<GetFriendFeedResponseDto> GetFriendFeedAsync(Guid currentUserId, GetFriendFeedRequestDto request);
}