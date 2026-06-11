using System.Security.Claims;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/missions")]
public class MissionsController : ControllerBase
{
    private readonly IMissionService _missionService;

    public MissionsController(IMissionService missionService)
    {
        _missionService = missionService;
    }
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    
    // Get missions of current user
    [HttpGet]
    public async Task<IActionResult> GetMyMissions()
    {
        var result = await _missionService.GetMyMissionsAsync(CurrentUserId);
        return result.IsSuccess
            ? Ok(new ApiResponse<object> (result.Data!, "Get missions successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // Get mission detail
    [HttpGet("{missionId:guid}")]
    public async Task<IActionResult> GetMissionDetail(Guid missionId)
    {
        var result = await _missionService.GetMissionDetailAsync(CurrentUserId, missionId);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(result.Data!, "Get mission detail successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
    
    // Get "in-progress" mission
    [HttpGet("in-progress")]
    public async Task<IActionResult> GetInProgressMissions()
    {
        var result = await _missionService.GetInProgressMissionsAsync(CurrentUserId);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(result.Data!, "Get in-progress missions successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
    
    // Get "completed" mission
    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedMissions()
    {
        var result = await _missionService.GetCompletedMissionsAsync(CurrentUserId);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(result.Data!, "Get completed missions successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
    
    // Get "expired" mission
    [HttpGet("expired")]
    public async Task<IActionResult> GetExpiredMissions()
    {
        var result = await _missionService.GetExpiredMissionsAsync(CurrentUserId);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(result.Data!, "Get expired missions successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
    
    // Claim reward
    [HttpPost("{missionId:guid}/claim")]
    public async Task<IActionResult> ClaimReward(Guid missionId)
    {
        var result = await _missionService.ClaimRewardAsync(CurrentUserId, missionId);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(missionId, "Reward claimed successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}