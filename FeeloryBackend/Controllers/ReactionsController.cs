using FeeloryBackend.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/reactions")]
public class ReactionsController : ControllerBase
{
    // Add reaction to post
    [HttpPost]
    public async Task<IActionResult> Add()
    {
        // Used to add an emote reaction to a post
        return Ok();
    }

    // Remove reaction from post
    [HttpDelete]
    public async Task<IActionResult> Remove()
    {
        // Used to remove reaction from a post
        return Ok();
    }

    // Get reactions of a post
    [HttpGet("post/{postId}")]
    public async Task<IActionResult> GetByPost(Guid postId)
    {
        // Used to get all reactions of a post
        return Ok();
    }
}