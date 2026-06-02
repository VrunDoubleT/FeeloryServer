using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Post;

namespace FeeloryBackend.Services.Interfaces;

public interface IPostService
{
    // Create diary post
    Task<Result<PostDto>> CreateAsync(Guid userId, CreatePostRequestDto request);

    // Update post
    Task<Result<PostDto>> UpdateAsync(Guid userId, Guid postId, UpdatePostRequestDto request);

    // Delete post
    Task<Result> DeleteAsync(Guid userId, Guid postId);

    // Get posts by user
    Task<GetMyPostsResponseDto> GetMyPostsAsync(Guid userId, GetMyPostsRequestDto request);
    
    // Get posts by id
    Task<Result<PostDetailDto?>>GetByIdAsync(Guid currentUserId, Guid postId);
    
    // Get my post feed
    Task<GetFriendFeedResponseDto> GetMyFeedAsync(Guid currentUserId, GetFriendFeedRequestDto request);

    // Get friend post feed
    Task<Result<GetFriendFeedResponseDto>> GetFriendFeedAsync(Guid currentUserId, Guid profileUserId, GetFriendFeedRequestDto request);
}