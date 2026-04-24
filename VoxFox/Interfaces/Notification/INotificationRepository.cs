namespace VoxFox.Interfaces.Notification;

public interface INotificationRepository
{
	Task<IList<Models.Entities.Notification>> GetByUserIdAsync(Guid userId);
	Task<Models.Entities.Notification?> GetByIdAsync(Guid id);
	Task<Models.Entities.Notification> AddAsync(Models.Entities.Notification notification);
	System.Threading.Tasks.Task MarkAsReadAsync(Guid notificationId);
	System.Threading.Tasks.Task MarkAllAsReadAsync(Guid userId);
	Task<int> GetUnreadCountAsync(Guid userId);
}
