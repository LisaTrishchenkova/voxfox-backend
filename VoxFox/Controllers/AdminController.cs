using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Interfaces.Admin;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetStatsAsync();
        return Ok(stats);
    }

    [HttpGet("moderators/stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModeratorsStats()
    {
        var stats = await _adminService.GetModeratorsStatsAsync();
        return Ok(stats);
    }

    [HttpPut("users/{id}/block")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BlockUser([FromRoute] Guid id, [FromBody] BlockUserRequest request)
    {
        var result = await _adminService.BlockUserAsync(id, request.Reason);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }

    [HttpPut("users/{id}/unblock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnblockUser([FromRoute] Guid id)
    {
        var result = await _adminService.UnblockUserAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }

    [HttpPut("courses/{id}/unpublish")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnpublishCourse([FromRoute] Guid id)
    {
        var result = await _adminService.UnpublishCourseAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }

    [HttpPut("courses/{id}/force-release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForceReleaseCourse([FromRoute] Guid id)
    {
        var result = await _adminService.ForceReleaseCourseAsync(id);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }
}

public class BlockUserRequest
{
    public string? Reason { get; set; }
}
