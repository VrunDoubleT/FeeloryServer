using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/day-shares")]
public class DaySharesController : ControllerBase
{
    // Create shared diary for a day
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        // Used to share multiple posts of a specific day
        return Ok();
    }
    
    // Update existing shared diary for a day
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id)
    {
        // Used to update post content
        return Ok();
    }

    // Get shared day detail
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Used to get full shared day timeline
        return Ok();
    }

    // Delete shared day
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // Used to delete a shared diary day
        return Ok();
    }
}