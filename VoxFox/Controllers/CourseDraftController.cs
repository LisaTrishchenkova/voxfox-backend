using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces.DraftCourse;
using VoxFox.Models.DTOs.Draft.CourseDraftDto;
using VoxFox.Models.DTOs.Draft.CreateCourseDraftDto;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/courses/{courseId}/draft")]
[Authorize(Roles = "Teacher,Admin")]
public class CourseDraftController : ControllerBase
{
 private readonly ICourseDraftService _draftService;

    public CourseDraftController(ICourseDraftService draftService)
    {
        _draftService = draftService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateDraft([FromRoute] Guid courseId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _draftService.CreateDraftAsync(courseId, userId.Value);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return CreatedAtAction(nameof(GetDraft), new { courseId }, result.Data);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseDraftDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDraft([FromRoute] Guid courseId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _draftService.GetDraftAsync(courseId, userId.Value);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpPut("{draftId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseDraftDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDraft(
        [FromRoute] Guid courseId,
        [FromRoute] Guid draftId,
        [FromBody] CreateCourseDraftDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _draftService.UpdateDraftAsync(draftId, dto, userId.Value);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpPost("{draftId}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitDraft(
        [FromRoute] Guid courseId,
        [FromRoute] Guid draftId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _draftService.SubmitDraftAsync(draftId, userId.Value);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }

    [HttpDelete("{draftId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDraft(
        [FromRoute] Guid courseId,
        [FromRoute] Guid draftId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _draftService.DeleteDraftAsync(draftId, userId.Value);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }
}
