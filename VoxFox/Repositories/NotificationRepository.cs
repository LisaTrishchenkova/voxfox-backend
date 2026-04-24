using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces.Notification;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class NotificationRepository : INotificationRepository
{
	private readonly ApplicationContext _context;

	public NotificationRepository(ApplicationContext context)
	{
		_context = context;
	}

	public async Task<IList<Notification>> GetByUserIdAsync(Guid userId)
	{
		var notifications = await _context.Notifications
			.Where(n => n.UserId == userId)
			.OrderByDescending(n => n.CreatedAt)
			.ToListAsync();

		return notifications;
	}

	public async Task<Notification?> GetByIdAsync(Guid id)
	{
		var notification = await _context.Notifications
			.FirstOrDefaultAsync(n => n.Id == id);

		return notification;
	}

	public async Task<Notification> AddAsync(Notification notification)
	{
		_context.Notifications.Add(notification);
		await _context.SaveChangesAsync();
		return notification;
	}

	public async Task MarkAsReadAsync(Guid notificationId)
	{
		var notification = await _context.Notifications.FindAsync(notificationId);
		if (notification == null) return;
		notification.IsRead = true;
		await _context.SaveChangesAsync();
	}

	public async Task MarkAllAsReadAsync(Guid userId)
	{
		await _context.Notifications
			.Where(n => n.UserId == userId && !n.IsRead)
			.ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
	}

	public async Task<int> GetUnreadCountAsync(Guid userId)
	{
		var count = await _context.Notifications
			.CountAsync(n => n.UserId == userId && !n.IsRead);

		return count;
	}
}
