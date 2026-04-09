using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces;
using VoxFox.Models.DTOs;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Controllers;

[ApiController]
[Authorize]
public class FavoriteController : ControllerBase
{
	private readonly IFavoriteService _favoriteService;


	public FavoriteController(IFavoriteService favoriteService)
	{
		_favoriteService = favoriteService;
	}

	[HttpGet("api/favorites")]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<FavoriteDto>))]
	public async Task<ActionResult<IList<FavoriteDto>>> GetFavorites()
	{
		var userId = User.GetUserId();
		if (userId == null) return Unauthorized();

		var result = await _favoriteService.GetUserFavoritesAsync(userId.Value);
		return Ok(result.Data);
	}

	[HttpPost("api/favorites")]
	[ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FavoriteDto))]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<FavoriteDto>> AddFavorite([FromBody] CreateFavoriteDto request)
	{
		var userId = User.GetUserId();
		if (userId == null) return Unauthorized();

		var result = await _favoriteService.AddFavoriteAsync(request.CourseId, userId.Value);
		if (!result.Success)
			return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

		return CreatedAtAction(nameof(GetFavorites), result.Data);
	}

	[HttpDelete("api/favorites/{courseId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> RemoveFavorite([FromRoute] Guid courseId)
	{
		var userId = User.GetUserId();
		if (userId == null) return Unauthorized();

		var result = await _favoriteService.RemoveFavoriteAsync(courseId, userId.Value);
		if (!result.Success)
			return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

		return NoContent();
	}
}
