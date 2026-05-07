using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces.Moderation;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/moderation")]
[Authorize(Roles = "Moderator,Admin")]
public class ModerationController : ControllerBase
{
    private readonly IModerationService _moderationService;

    public ModerationController(IModerationService moderationService)
    {
        _moderationService = moderationService;
    }

    [HttpPost("courses/{courseId}/claim")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ClaimCourse([FromRoute] Guid courseId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _moderationService.ClaimCourseAsync(courseId, userId.Value);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }

    [HttpPost("courses/{courseId}/release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReleaseCourse([FromRoute] Guid courseId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _moderationService.ReleaseCourseAsync(courseId, userId.Value);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }

    [HttpGet("courses/{courseId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseForReview([FromRoute] Guid courseId)
    {
        var result = await _moderationService.GetCourseForReviewAsync(courseId);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpGet("stats/my")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyStats()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var stats = await _moderationService.GetMyStatsAsync(userId.Value);
        return Ok(stats);
    }
}
