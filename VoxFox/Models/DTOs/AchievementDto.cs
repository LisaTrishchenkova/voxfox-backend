namespace VoxFox.Models;

public class AchievementDto
{
	public Guid Id { get; set; }
	public string Code { get; set; } = null!;
	public string Title { get; set; } = null!;
	public string Description { get; set; } = null!;
	public string Icon { get; set; } = null!;

	/// <summary>null — ещё не получено</summary>
	public DateTime? EarnedAt { get; set; }

	public bool IsEarned => EarnedAt.HasValue;
}

/// <summary>Возвращается из сервиса когда ачивка только что получена — для попапа на фронте</summary>
public class NewAchievementDto
{
	public string Code { get; set; } = null!;
	public string Title { get; set; } = null!;
	public string Description { get; set; } = null!;
	public string Icon { get; set; } = null!;
	public DateTime EarnedAt { get; set; }
}
