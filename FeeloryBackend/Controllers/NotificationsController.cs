using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Get notifications of current user
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CursorPaginationRequest request)
    {
        var result = await _notificationService.GetByUserAsync(CurrentUserId, request);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // Mark notification as read
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _notificationService.MarkAsReadAsync(CurrentUserId, id);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(new { message = "Marked as read" }, "Notification marked as read"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // Mark all as read
    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _notificationService.MarkAllAsReadAsync(CurrentUserId);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(new { updatedCount = result.Data }, "All notifications marked as read"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}