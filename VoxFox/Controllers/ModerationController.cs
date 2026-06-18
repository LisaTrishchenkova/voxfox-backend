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
    public async Task<IActionResult> ClaimCourse([FromRoute] Guid courseId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _moderationService.ClaimCourseAsync(courseId, userId.Value);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }

    [HttpPost("courses/{courseId}/release")]
    public async Task<IActionResult> ReleaseCourse([FromRoute] Guid courseId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _moderationService.ReleaseCourseAsync(courseId, userId.Value);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }

    [HttpGet("courses/{courseId}")]
    public async Task<IActionResult> GetCourseForReview([FromRoute] Guid courseId)
    {
        var result = await _moderationService.GetCourseForReviewAsync(courseId);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return Ok(result.Data);
    }

    [HttpGet("stats/my")]
    public async Task<IActionResult> GetMyStats()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var stats = await _moderationService.GetMyStatsAsync(userId.Value);
        return Ok(stats);
    }

    [HttpGet("drafts/pending")]
    public async Task<IActionResult> GetPendingDrafts()
    {
        var drafts = await _draftService.GetPendingDraftsAsync();
        return Ok(drafts);
    }

    [HttpGet("drafts/{draftId}")]
    public async Task<IActionResult> GetDraftForReview([FromRoute] Guid draftId)
    {
        var result = await _draftService.GetDraftForReviewAsync(draftId);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return Ok(result.Data);
    }

    [HttpPost("drafts/{draftId}/claim")]
    public async Task<IActionResult> ClaimDraft([FromRoute] Guid draftId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _draftService.ClaimDraftAsync(draftId, userId.Value);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }

    [HttpPost("drafts/{draftId}/release")]
    public async Task<IActionResult> ReleaseDraft([FromRoute] Guid draftId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _draftService.ReleaseDraftAsync(draftId, userId.Value);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }

    [HttpPut("drafts/{draftId}/approve")]
    public async Task<IActionResult> ApproveDraft([FromRoute] Guid draftId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _draftService.ApproveDraftAsync(draftId, userId.Value);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }

    [HttpPut("drafts/{draftId}/reject")]
    public async Task<IActionResult> RejectDraft(
        [FromRoute] Guid draftId,
        [FromBody] RejectDraftRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _draftService.RejectDraftAsync(draftId, request.Reason, userId.Value);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }
}

public class RejectDraftRequest
{
    public string? Reason { get; set; }
}
