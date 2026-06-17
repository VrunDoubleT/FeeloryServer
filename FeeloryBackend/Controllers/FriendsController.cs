using System.Security.Claims;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.Friend;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Authorize]
[Route("api/friends")]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendsController(IFriendService friendService)
        => _friendService = friendService;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // POST api/friends/request
    [HttpPost("request")]
    public async Task<IActionResult> SendRequest([FromBody] SendFriendRequestDto dto)
    {
        var result = await _friendService.SendRequestAsync(CurrentUserId, dto.ReceiverId);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "The friend request has been sent successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // POST api/friends/accept
    [HttpPost("accept")]
    public async Task<IActionResult> Accept([FromBody] AcceptFriendRequestDto dto)
    {
        var result = await _friendService.AcceptRequestAsync(CurrentUserId, dto.RequestId);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "Friend request accepted successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // POST api/friends/reject
    [HttpPost("reject")]
    public async Task<IActionResult> Reject([FromBody] RejectFriendRequestDto dto)
    {
        var result = await _friendService.RejectRequestAsync(CurrentUserId, dto.RequestId);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "Friend request has been rejected"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // DELETE api/friends/{friendUserId}
    [HttpDelete("{friendUserId:guid}")]
    public async Task<IActionResult> Remove(Guid friendUserId)
    {
        var result = await _friendService.RemoveFriendAsync(CurrentUserId, friendUserId);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "Đã xóa bạn bè thành công."))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // GET api/friends
    [HttpGet]
    public async Task<IActionResult> GetFriends(
        [FromQuery] CursorPaginationRequest request)
    {
        var result = await _friendService
            .GetFriendsAsync(CurrentUserId, request);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // GET api/friends/requests/pending
    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPendingRequests(
        [FromQuery] CursorPaginationRequest request)
    {
        var result = await _friendService
            .GetPendingRequestsAsync(
                CurrentUserId,
                request);

        return result.IsSuccess
            ? Ok(result.Data!)
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}