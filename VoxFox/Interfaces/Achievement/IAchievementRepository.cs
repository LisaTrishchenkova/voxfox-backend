using VoxFox.Models.Entities;

namespace VoxFox.Interfaces.Achievement;

public interface IAchievementRepository
{
	Task<Models.Entities.Achievement?> GetByCodeAsync(string code);
	Task<List<Models.Entities.Achievement>> GetAllAsync();
	Task<List<UserAchievement>> GetUserAchievementsAsync(Guid userId);
	Task<bool> HasAchievementAsync(Guid userId, string code);
	Task<UserAchievement> AddUserAchievementAsync(UserAchievement ua);
}
