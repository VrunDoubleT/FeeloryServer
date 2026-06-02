using System.Security.Claims;
using FeeloryBackend.Models.DTOs.DayShare;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/day-shares")]
[Authorize]
public class DaySharesController : ControllerBase
{
    private readonly IDayShareService _dayShareService;

    public DaySharesController(IDayShareService dayShareService)
    {
        _dayShareService = dayShareService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDayShareRequestDto dto)
    {
        var result = await _dayShareService.CreateAsync(CurrentUserId, dto);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "DayShare created successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    //Update
    [HttpPatch]
    public async Task<IActionResult> Update([FromBody] UpdateDayShareRequestDto dto)
    {
        var result = await _dayShareService
            .UpdateAsync(CurrentUserId, dto);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(
                null, "DayShare updated successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    //getid
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _dayShareService
            .GetByIdAsync(CurrentUserId, id);

        return result.IsSuccess
            ? Ok(new ApiResponse<DayShareDetailDto>(
                result.Data, "Success"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    //dele
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _dayShareService
            .DeleteAsync(CurrentUserId, id);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(
                null, "DayShare deleted successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    //get feed
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed(
        [FromQuery] string? cursor = null,
        [FromQuery] int pageSize = 10)
    {
        var result = await _dayShareService
            .GetFeedAsync(CurrentUserId, cursor, pageSize);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
    
    // GET /api/day-shares/user/{userId}
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserFeed(
        Guid userId,
        [FromQuery] string? cursor   = null,
        [FromQuery] int     pageSize = 10)
    {
        var result = await _dayShareService
            .GetUserFeedAsync(CurrentUserId, userId, cursor, pageSize);

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}