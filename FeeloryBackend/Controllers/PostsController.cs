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
        var result = await _postService.CreateAsync(CurrentUserId, request);
        return result.IsSuccess 
            ? Ok( new ApiResponse<object>( result.Data!, "Post created successfully")) 
            : BadRequest( new ApiErrorResponse(result.Error!));
    }

    // Update existing post
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequestDto request)
    {
        var result = await _postService.UpdateAsync(CurrentUserId, id, request);
        
        return result.IsSuccess 
            ? Ok( new ApiResponse<object>( result.Data!, "Post updated successfully")) 
            : BadRequest( new ApiErrorResponse(result.Error!));
    }

    // Delete post
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _postService.DeleteAsync(CurrentUserId, id);
        return result.IsSuccess 
            ? Ok( new ApiResponse<object>( id, "Post deleted successfully")) 
            : BadRequest( new ApiErrorResponse(result.Error!));
    }

    // Get posts of a user
    [HttpGet("me")]
    public async Task<IActionResult> GetMyPosts([FromQuery] GetMyPostsRequestDto request)
    {
        var result = await _postService.GetMyPostsAsync(CurrentUserId, request);
        return Ok(new ApiResponse<object>(result, "Get my posts successfully"));
    }
    
    // Get posts by ID
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _postService.GetByIdAsync(CurrentUserId, id);

        if (result.IsSuccess)
            return Ok(new ApiResponse<PostDetailDto>(result.Data!, "Get post successfully"));

        if (result.Error == "You do not have permission to get this post")
            return StatusCode(403, new ApiErrorResponse(result.Error));

        return BadRequest(new ApiErrorResponse(result.Error!));
    }
    
    // Get my post feed
    [HttpGet("feed")]
    public async Task<IActionResult> GetMyFeed([FromQuery] GetFriendFeedRequestDto request)
    {
        var result = await _postService.GetMyFeedAsync(CurrentUserId, request);
        return Ok(new ApiResponse<object>(result, "Get feed successfully")
        );
    }
    
    // Get friend post feed
    [HttpGet("feed/{id:guid}")]
    public async Task<IActionResult> GetFriendFeed(Guid id, [FromQuery] GetFriendFeedRequestDto request)
    {
        var result = await _postService.GetFriendFeedAsync(CurrentUserId, id, request);
        
        return result.IsSuccess
            ?Ok(new ApiResponse<object>(result, "Get feed successfully"))
            : BadRequest( new ApiErrorResponse(result.Error!));
    }
}