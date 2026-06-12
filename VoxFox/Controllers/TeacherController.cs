using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces.Teacher;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher,Admin")]
public class TeacherController: ControllerBase
{
	private readonly ITeacherService _teacherService;

	public TeacherController(ITeacherService teacherService)
	{
		_teacherService = teacherService;
	}

	/// <summary>Сводная статистика преподавателя</summary>
	[HttpGet("stats")]
	public async Task<IActionResult> GetStats()
	{
		var userId = User.GetUserId();
		if (userId == null) return Unauthorized();

		var result = await _teacherService.GetStatsAsync(userId.Value);
		if (!result.Success)
			return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

		return Ok(result.Data);
	}

	/// <summary>Детальная статистика по каждому курсу преподавателя</summary>
	[HttpGet("courses/stats")]
	public async Task<IActionResult> GetCourseStats()
	{
		var userId = User.GetUserId();
		if (userId == null) return Unauthorized();

		var result = await _teacherService.GetCourseStatsAsync(userId.Value);
		if (!result.Success)
			return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

		return Ok(result.Data);
	}
}
