using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/calendar")]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;
    private readonly ICurrentUserService _currentUserService;
    public CalendarController(ICalendarService calendarService,  ICurrentUserService currentUserService)
    {
        _calendarService = calendarService;
        _currentUserService = currentUserService;
    }
    // Get posts by month
    [HttpGet("month")]
    public async Task<IActionResult> GetMonthlyData([FromQuery] int year, [FromQuery] int month)
    {
        if (year <= 0 || month < 1 || month > 12)
        {
            return BadRequest(new ApiErrorResponse("Invalid year or month. Month must be 1-12."));
        }
        var result = await _calendarService.GetMonthlyAsync(_currentUserService.GetUserId() ,month, year);
        if (!result.IsSuccess)
            return BadRequest(new ApiErrorResponse(result.Error!));

        var data = result.Data!.PostCountPerDay
            .OrderBy(kv => kv.Key)
            .Select(kv => new { day = kv.Key, count = kv.Value })
            .ToList();

        return Ok(new ApiResponse<object>(data, "Monthly calendar data retrieved successfully"));
    }

    // Get posts by specific day
    [HttpGet("day")]
    public async Task<IActionResult> GetDailyData([FromQuery] DateOnly date)
    {
        var result = await _calendarService.GetDailyAsync(_currentUserService.GetUserId(), date);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(result.Data!.Posts, "Daily timeline retrieved successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}