using FeeloryBackend.Models.DTOs.Reaction;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/reactions")]
public class ReactionsController : ControllerBase
{
    private readonly IReactionService _reactionService;
    private readonly ICurrentUserService _currentUserService;

    public ReactionsController(
        IReactionService reactionService,
        ICurrentUserService currentUserService)
    {
        _reactionService = reactionService;
        _currentUserService = currentUserService;
    }

    private Guid CurrentUserId => _currentUserService.GetUserId();

    [HttpPost("posts")]
    public async Task<IActionResult> AddToPost(
        [FromBody] AddPostReactionRequestDto dto)
    {
        var result = await _reactionService.AddToPostAsync(
            CurrentUserId,
            dto.PostId,
            dto.EmoteId);

        return result.IsSuccess
            ? Ok(new ApiResponse<ReactionResponseDto>(
                result.Data!,
                "Reaction added"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    [HttpDelete("posts/{postId:guid}")]
    public async Task<IActionResult> RemoveFromPost(Guid postId)
    {
        var result = await _reactionService.RemoveFromPostAsync(
            CurrentUserId,
            postId);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(
                null,
                "Reaction removed"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    [HttpGet("posts/{postId:guid}")]
    public async Task<IActionResult> GetByPost(Guid postId)
    {
        var result = await _reactionService.GetByPostAsync(
            CurrentUserId,
            postId);

        return result.IsSuccess
            ? Ok(new ApiResponse<List<ReactionGroupDto>>(
                result.Data!,
                "Success"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}