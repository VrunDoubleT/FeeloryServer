using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Register new user account
    [HttpPost("register")]
    public async Task<IActionResult> Register()
    {
        // Used to create a new user account
        return Ok();
    }

    // Login and return authentication token
    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        // Used to authenticate user and return JWT token
        return Ok();
    }

    // Get current logged-in user information
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        // Used to get current authenticated user profile
        return Ok();
    }
}