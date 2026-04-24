using VoxFox.Enums;

namespace VoxFox.Models.DTOs.Notification;

public class NotificationDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = null!;
	public string Message { get; set; } = null!;
	public NotificationType Type { get; set; }
	public bool IsRead { get; set; }
	public Guid? RelatedEntityId { get; set; }
	public DateTime CreatedAt { get; set; }
}
