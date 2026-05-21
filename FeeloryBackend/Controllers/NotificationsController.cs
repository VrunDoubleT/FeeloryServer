using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    // Get notifications of current user
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Used to retrieve all notifications
        return Ok();
    }

    // Mark notification as read
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        // Used to mark a notification as read
        return Ok();
    }
}