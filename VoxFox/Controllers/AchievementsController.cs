using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces.Achievement;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/achievements")]
[Authorize]
public class AchievementsController : ControllerBase
{
	private readonly IAchievementService _achievementService;

	public AchievementsController(IAchievementService achievementService)
	{
		_achievementService = achievementService;
	}

	/// <summary>Все достижения текущего пользователя (полученные + заблокированные)</summary>
	[HttpGet("my")]
	public async Task<IActionResult> GetMyAchievements()
	{
		var userId = User.GetUserId();
		if (userId == null) return Unauthorized();

		var result = await _achievementService.GetUserAchievementsAsync(userId.Value);
		if (!result.Success)
			return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

		return Ok(result.Data);
	}
}
