using System.Security.Claims;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/heartbeat")]
[Authorize]
public class HeartbeatController : ControllerBase
{
    private readonly IHeartbeatService _heartbeatService;

    public HeartbeatController(
        IHeartbeatService heartbeatService)
    {
        _heartbeatService = heartbeatService;
    }
    
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Track()
    {
        await _heartbeatService.TrackLoginAsync(CurrentUserId);
        return Ok();
    }
}