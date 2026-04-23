using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces.Review;
using VoxFox.Models.DTOs;

namespace VoxFox.Controllers;

[ApiController]
[Authorize]
public class ReviewsController : ControllerBase
{
	private readonly IReviewService _reviewService;

	public ReviewsController(IReviewService reviewService)
	{
		_reviewService = reviewService;
	}

	[HttpPost("api/Courses/{courseId}/reviews")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReviewDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewDto>> CreateReview(
        [FromRoute] Guid courseId,
        [FromBody] CreateReviewDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _reviewService.CreateReviewAsync(courseId, userId.Value, dto);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return CreatedAtAction(nameof(GetCourseReviews), new { courseId }, result.Data);
    }

    [HttpGet("api/Courses/{courseId}/reviews")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<ReviewDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IList<ReviewDto>>> GetCourseReviews(
        [FromRoute] Guid courseId)
    {
        var result = await _reviewService.GetCourseReviewsAsync(courseId);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpPut("api/reviews/{reviewId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> UpdateReview(
        [FromRoute] Guid reviewId,
        [FromBody] UpdateReviewDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _reviewService.UpdateReviewAsync(reviewId, userId.Value, dto);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpDelete("api/reviews/{reviewId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview([FromRoute] Guid reviewId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var role = User.GetUserRole()?.ToString() ?? "Student";

        var result = await _reviewService.DeleteReviewAsync(reviewId, userId.Value, role);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }
}
