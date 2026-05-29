using System.Security.Claims;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/emote-packages")]
public class EmotePackagesController : ControllerBase
{
    private readonly IEmotePackageService _packageService;

    public EmotePackagesController(IEmotePackageService packageService) => _packageService = packageService;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _packageService.GetAllAsync();
        return result.IsSuccess
            ? Ok(new ApiResponse<List<EmotePackageDto>>(result.Data!))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _packageService.GetByIdAsync(id);
        return result.IsSuccess
            ? Ok(new ApiResponse<EmotePackageDto>(result.Data!))
            : NotFound(new ApiErrorResponse(result.Error!));
    }

    [HttpPost("{id:guid}/unlock")]
    [Authorize]
    public async Task<IActionResult> Unlock(Guid id)
    {
        var result = await _packageService.UnlockAsync(CurrentUserId, id);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "Package unlocked successfully."))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    [HttpGet("my-packages")]
    [Authorize]
    public async Task<IActionResult> GetMyPackages()
    {
        var result = await _packageService.GetUserPackagesAsync(CurrentUserId);
        return result.IsSuccess
            ? Ok(new ApiResponse<List<EmotePackageDto>>(result.Data!))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}