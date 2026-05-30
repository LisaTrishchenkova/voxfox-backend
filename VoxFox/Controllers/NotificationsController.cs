using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces.Notification;
using VoxFox.Models.DTOs.Notification;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
	private readonly INotificationService _notificationService;

	public NotificationsController(INotificationService notificationService)
	{
		_notificationService = notificationService;
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<NotificationDto>))]
	public async Task<ActionResult<IList<NotificationDto>>> GetMyNotifications()
	{
		var userId = User.GetUserId();
		if (userId == null) return Unauthorized();

		var result = await _notificationService.GetMyNotificationsAsync(userId.Value);
		return Ok(result.Data);
	}

	[HttpGet("unread-count")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<int>> GetUnreadCount()
	{
		var userId = User.GetUserId();
		if (userId == null) return Unauthorized();

		var result = await _notificationService.GetUnreadCountAsync(userId.Value);
		return Ok(new { count = result.Data });
	}

	[HttpPut("{notificationId}/read")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status403Forbidden)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> MarkAsRead([FromRoute] Guid notificationId)
	{
		var userId = User.GetUserId();
		if (userId == null) return Unauthorized();

		var result = await _notificationService.MarkAsReadAsync(notificationId, userId.Value);
		if (!result.Success)
			return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

		return NoContent();
	}

	[HttpPut("read-all")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> MarkAllAsRead()
	{
		var userId = User.GetUserId();
		if (userId == null) return Unauthorized();

		await _notificationService.MarkAllAsReadAsync(userId.Value);
		return NoContent();
	}
}
