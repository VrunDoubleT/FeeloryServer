using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    // Create new diary post
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        // Used to create a new diary post with image + mood emote
        return Ok();
    }

    // Update existing post
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id)
    {
        // Used to update post content
        return Ok();
    }

    // Delete post
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // Used to delete a post
        return Ok();
    }

    // Get posts of a user
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        // Used to get all posts of a specific user
        return Ok();
    }
}