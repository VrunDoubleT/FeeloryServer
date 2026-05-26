using System.Security.Claims;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.User;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
        => _userService = userService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Get user profile by user id
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        var result = await _userService.GetProfileAsync(CurrentUserId, userId);

        return result.IsSuccess
            ? Ok(new ApiResponse<UserProfileDto>(result.Data!, "User profile retrieved successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // Update user profile information
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequestDto dto)
    {
        var result = await _userService.UpdateProfileAsync(CurrentUserId, dto);

        return result.IsSuccess
            ? Ok(new ApiResponse<UserProfileDto>(result.Data!, "Profile updated successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // Search users by keyword
    [HttpGet("search/name")]
    public async Task<IActionResult> SearchByName(
        [FromQuery] string q,
        [FromQuery] CursorPaginationRequest request)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new ApiErrorResponse("Search query 'q' is required."));

        var result = await _userService.SearchByDisplayNameAsync(CurrentUserId, q, request);

        return result.IsSuccess
            ? Ok(new ApiResponse<CursorPaginationResponse<UserProfileDto>>(result.Data!, "Search completed"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // GET api/users/search/username?username=@keyword
    [HttpGet("search/username")]
    public async Task<IActionResult> SearchByUsername(
        [FromQuery] string username,
        [FromQuery] CursorPaginationRequest request)
    {
        if (string.IsNullOrWhiteSpace(username))
            return BadRequest(new ApiErrorResponse("Username search query is required."));

        var result = await _userService.SearchByUsernameAsync(CurrentUserId, username, request);

        return result.IsSuccess
            ? Ok(new ApiResponse<CursorPaginationResponse<UserProfileDto>>(result.Data!, "Search completed"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}