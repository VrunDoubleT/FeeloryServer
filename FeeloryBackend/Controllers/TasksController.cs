using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    // Get all available tasks
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Used to retrieve all system tasks
        return Ok();
    }

    // Get user task progress
    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress()
    {
        // Used to get current user task progress
        return Ok();
    }
}