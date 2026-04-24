using VoxFox.Enums;
using VoxFox.Interfaces.Notification;
using VoxFox.Models.DTOs.Notification;
using VoxFox.Models.Entities;

namespace VoxFox.Services;

public class NotificationService : INotificationService
{
	private readonly INotificationRepository _notificationRepository;

	public NotificationService(INotificationRepository notificationRepository)
	{
		_notificationRepository = notificationRepository;
	}

	public async Task<ServiceResult<IList<NotificationDto>>> GetMyNotificationsAsync(Guid userId)
	{
		var notifications = await _notificationRepository.GetByUserIdAsync(userId);
		return ServiceResult<IList<NotificationDto>>.Ok(
			notifications.Select(MapToDto).ToList());
	}

	public async Task<ServiceResult<bool>> MarkAsReadAsync(Guid notificationId, Guid userId)
	{
		var notification = await _notificationRepository.GetByIdAsync(notificationId);
		if (notification == null)
			return ServiceResult<bool>.Fail("Уведомление не найдено", StatusCodes.Status404NotFound);

		if (notification.UserId != userId)
			return ServiceResult<bool>.Fail("Нет доступа", StatusCodes.Status403Forbidden);

		await _notificationRepository.MarkAsReadAsync(notificationId);
		return ServiceResult<bool>.Ok(true);
	}

	public async Task<ServiceResult<bool>> MarkAllAsReadAsync(Guid userId)
	{
		await _notificationRepository.MarkAllAsReadAsync(userId);
		return ServiceResult<bool>.Ok(true);
	}

	public async Task<ServiceResult<int>> GetUnreadCountAsync(Guid userId)
	{
		var count = await _notificationRepository.GetUnreadCountAsync(userId);
		return ServiceResult<int>.Ok(count);
	}

	public async Task SendAsync(Guid userId, string title, string message, NotificationType type, Guid? relatedEntityId = null)
	{
		var notification = new Notification
		{
			UserId = userId,
			Title = title,
			Message = message,
			Type = type,
			RelatedEntityId = relatedEntityId,
			CreatedAt = DateTime.UtcNow
		};

		await _notificationRepository.AddAsync(notification);
	}
	private static NotificationDto MapToDto(Notification n) => new()
	{
		Id = n.Id,
		Title = n.Title,
		Message = n.Message,
		Type = n.Type,
		IsRead = n.IsRead,
		RelatedEntityId = n.RelatedEntityId,
		CreatedAt = n.CreatedAt
	};
}
