using VoxFox.Enums;

namespace VoxFox.Models.Entities;

public class Notification
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public string Title { get; set; } = null!;
	public string Message { get; set; } = null!;
	public NotificationType Type { get; set; }
	public bool IsRead { get; set; } = false;
	public Guid? RelatedEntityId { get; set; }
	public Guid? RelatedCourseId { get; set; }
	public DateTime CreatedAt { get; set; }

	public User? User { get; set; }
}
