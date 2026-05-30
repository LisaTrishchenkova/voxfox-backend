using VoxFox.Enums;
using VoxFox.Models.DTOs.Notification;

namespace VoxFox.Interfaces.Notification;

public interface INotificationService
{
	Task<ServiceResult<IList<NotificationDto>>> GetMyNotificationsAsync(Guid userId);
	Task<ServiceResult<bool>> MarkAsReadAsync(Guid notificationId, Guid userId);
	Task<ServiceResult<bool>> MarkAllAsReadAsync(Guid userId);
	Task<ServiceResult<int>> GetUnreadCountAsync(Guid userId);
	System.Threading.Tasks.Task SendAsync(Guid userId, string title, string message, NotificationType type,
		Guid? relatedEntityId = null, Guid? relatedCourseId = null);
}
