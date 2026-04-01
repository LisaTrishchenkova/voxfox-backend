using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces.Enrollment;
using VoxFox.Models.DTOs;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
	 private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(EnrollmentDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EnrollmentDto>> Enroll(
	    [FromBody] CreateEnrollmentDto dto)
    {
	    var userId = User.GetUserId();

	    var result = await _enrollmentService.EnrollAsync(dto.CourseId, userId.Value);

	    if (!result.Success)
		    return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

	    return CreatedAtAction(nameof(GetUserEnrollments), result.Data);
    }

    [HttpDelete("{enrollmentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelEnrollment(
        [FromRoute] Guid enrollmentId)
    {
        var userId = User.GetUserId();

        var result = await _enrollmentService.CancelEnrollmentAsync(enrollmentId, userId.Value);

        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<EnrollmentDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IList<EnrollmentDto>>> GetUserEnrollments()
    {
        var userId = User.GetUserId();

        var result = await _enrollmentService.GetUserEnrollmentsAsync(userId.Value);

        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }
}
