using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/emotes")]
public class EmotesController : ControllerBase
{
    // Get all available emotes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Used to retrieve all global emotes
        return Ok();
    }

    // Get emote by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Used to get emote detail
        return Ok();
    }
}