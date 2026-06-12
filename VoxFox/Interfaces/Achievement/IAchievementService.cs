using VoxFox.Models;

namespace VoxFox.Interfaces.Achievement;

public interface IAchievementService
{
	/// <summary>Все достижения пользователя (полученные + заблокированные)</summary>
	Task<ServiceResult<List<AchievementDto>>> GetUserAchievementsAsync(Guid userId);

	/// <summary>
	/// Проверить и выдать подходящие ачивки после события.
	/// Возвращает список только что полученных — для попапа на фронте.
	/// </summary>
	Task<List<NewAchievementDto>> CheckAndAwardAsync(Guid userId, AchievementTrigger trigger);
}

public enum AchievementTrigger
{
	LessonCompleted,
	CourseEnrolled,
	CourseCompleted,
	CertificateIssued,
	ReviewCreated,
}
