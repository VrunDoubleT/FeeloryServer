using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/calendar")]
public class CalendarController : ControllerBase
{
    // Get posts by month
    [HttpGet("month")]
    public async Task<IActionResult> GetMonthlyData()
    {
        // Used to get diary posts grouped by month
        return Ok();
    }

    // Get posts by specific day
    [HttpGet("day")]
    public async Task<IActionResult> GetDailyData()
    {
        // Used to get timeline of posts in a specific day
        return Ok();
    }
}