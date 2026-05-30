using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces.DraftCourse;
using VoxFox.Interfaces.Moderation;
using VoxFox.Models.DTOs.Draft.CourseDraftDto;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/moderation")]
[Authorize(Roles = "Moderator,Admin")]
public class ModerationController : ControllerBase
{
    private readonly IModerationService _moderationService;
    private readonly ICourseDraftService _draftService;


    public ModerationController(IModerationService moderationService, ICourseDraftService draftService)
    {
        _moderationService = moderationService;
        _draftService = draftService;
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

     [HttpGet("drafts/pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingDrafts()
    {
        var drafts = await _draftService.GetPendingDraftsAsync();
        return Ok(drafts);
    }

    [HttpGet("drafts/{draftId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseDraftDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDraftForReview([FromRoute] Guid draftId)
    {
        var result = await _draftService.GetDraftForReviewAsync(draftId);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpPut("drafts/{draftId}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveDraft([FromRoute] Guid draftId)
    {
        var result = await _draftService.ApproveDraftAsync(draftId);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }

    [HttpPut("drafts/{draftId}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectDraft(
        [FromRoute] Guid draftId,
        [FromBody] RejectDraftRequest request)
    {
        var result = await _draftService.RejectDraftAsync(draftId, request.Reason);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }
}

public class RejectDraftRequest
{
    public string? Reason { get; set; }
}
