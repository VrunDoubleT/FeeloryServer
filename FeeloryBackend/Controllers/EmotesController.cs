using System.Security.Claims;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/emotes")]
public class EmotesController : ControllerBase
{
    private readonly IEmoteService _emoteService;

    public EmotesController(IEmoteService emoteService)
    {
        _emoteService = emoteService;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Get all available emotes
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllGrouped()
    {
        var result = await _emoteService.GetAllGroupedAsync();

        return result.IsSuccess
            ? Ok(new ApiResponse<Dictionary<string, List<EmoteDto>>>(result.Data!, "Retrieved all emotes successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // Get emote by id
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _emoteService.GetByIdAsync(id);

        return result.IsSuccess
            ? Ok(new ApiResponse<EmoteDto>(result.Data!, "Retrieved emote detail successfully"))
            : NotFound(new ApiErrorResponse(result.Error!));
    }

    // GET: api/emotes/my-emotes
    [HttpGet("my-emotes")]
    [Authorize]
    public async Task<IActionResult> GetUserEmotes()
    {
        var result = await _emoteService.GetUserEmotesAsync(CurrentUserId);

        return result.IsSuccess
            ? Ok(new ApiResponse<List<UserEmoteDto>>(result.Data!, "Retrieved user emotes successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // GET: api/emotes/recent
    [HttpGet("recent")]
    [Authorize]
    public async Task<IActionResult> GetRecentEmotes([FromQuery] int limit = 10)
    {
        var result = await _emoteService.GetRecentEmotesAsync(CurrentUserId, limit);

        return result.IsSuccess
            ? Ok(new ApiResponse<List<EmoteDto>>(result.Data!, "Retrieved recent emotes successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}