namespace VoxFox.Models.Entities;

public class UserAchievement
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public User User { get; set; } = null!;

	public Guid AchievementId { get; set; }
	public Achievement Achievement { get; set; } = null!;

	public DateTime EarnedAt { get; set; }
}
