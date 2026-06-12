namespace VoxFox.Models.Entities;

public class Achievement
{
	public Guid Id { get; set; }

	/// <summary>Уникальный код: first_lesson, course_3, и т.д.</summary>
	public string Code { get; set; } = null!;

	public string Title { get; set; } = null!;
	public string Description { get; set; } = null!;

	/// <summary>Эмодзи-иконка для отображения на фронте</summary>
	public string Icon { get; set; } = null!;

	public ICollection<UserAchievement> UserAchievements { get; set; } = [];
}
