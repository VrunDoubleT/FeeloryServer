using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    // Get user profile by user id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Used to retrieve user profile information
        return Ok();
    }

    // Update user profile information
    [HttpPut]
    public async Task<IActionResult> Update()
    {
        // Used to update user display name, avatar, etc.
        return Ok();
    }

    // Search users by keyword
    [HttpGet("search")]
    public async Task<IActionResult> Search(string keyword)
    {
        // Used to search users by username or display name
        return Ok();
    }
}