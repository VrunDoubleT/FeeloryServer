using System.Security.Claims;
using FeeloryBackend.Models.DTOs.Post;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Authorize]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService) => _postService = postService;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Create post
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreatePostRequestDto request)
    {
        var postId = await _postService.CreateAsync(CurrentUserId, request);
        
        return postId != Guid.Empty
            ? Ok(new ApiResponse<object>(postId, "Post created successfully"))
            : BadRequest(new ApiErrorResponse("Create post failed"));
    }

    // Update existing post
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequestDto request)
    {
        var updated = await _postService.UpdateAsync(CurrentUserId, id, request);

        return updated
            ? Ok(new ApiResponse<object>(id, "Post updated successfully"))
            : BadRequest(new ApiErrorResponse("Update post failed"));
    }

    // Delete post
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _postService.DeleteAsync(CurrentUserId, id);

        return deleted
            ? Ok(new ApiResponse<object>(id, "Post deleted successfully"))
            : BadRequest(new ApiErrorResponse("Delete post failed"));
    }

    // Get posts of a user
    [HttpGet("me")]
    public async Task<IActionResult> GetMyPosts([FromQuery] GetMyPostsRequestDto request)
    {
        var result = await _postService.GetMyPostsAsync(CurrentUserId, request);

        return Ok(new ApiResponse<object>(result, "Get my posts successfully")
        );
    }
    
    // Get posts by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _postService.GetByIdAsync(CurrentUserId, id);

            if (result == null)
            {
                return NotFound(new ApiErrorResponse("Post not found"));
            }

            return Ok(new ApiResponse<object>(result, "Get post successfully"));
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ApiErrorResponse("Forbidden"));
        }
    }
    
    // Get friend post feed
    [HttpGet("feed")]
    public async Task<IActionResult> GetFriendFeed([FromQuery] GetFriendFeedRequestDto request)
    {
        var result = await _postService.GetFriendFeedAsync(CurrentUserId, request);

        return Ok(new ApiResponse<object>(result, "Get feed successfully")
        );
    }
}