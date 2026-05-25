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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDayShareRequestDto dto)
    {
        var result = await _dayShareService.CreateAsync(CurrentUserId, dto);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "DayShare created successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}