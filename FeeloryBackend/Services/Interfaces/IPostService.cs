using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.Post;
using FeeloryBackend.Responses;

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
    Task<Result<CursorPaginationResponse<MyPostItemDto>>> GetMyPostsAsync(Guid userId, GetMyPostsRequestDto request);
    
    // Get posts by id
    Task<Result<PostDetailDto?>>GetByIdAsync(Guid currentUserId, Guid postId);
    
    // Get my post feed
    Task<Result<CursorPaginationResponse<PostFeedItemDto>>> GetMyFeedAsync(Guid currentUserId, CursorPaginationRequest request);

    // Get friend post feed
    Task<Result<CursorPaginationResponse<PostFeedItemDto>>> GetFriendFeedAsync(Guid currentUserId, Guid profileUserId, CursorPaginationRequest request);
    
    /// <summary>
    /// Retrieves a post by its identifier.
    /// Returns null if the post does not exist.
    /// </summary>
    Task<PostDetailDto?> FindByIdAsync(Guid postId);
}