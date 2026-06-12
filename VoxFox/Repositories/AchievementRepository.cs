using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces.Achievement;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories
{
	public class AchievementRepository : IAchievementRepository
	{
		private readonly ApplicationContext _context;
		private readonly ILogger<AchievementRepository> _logger;

		public AchievementRepository(ApplicationContext context, ILogger<AchievementRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<Achievement?> GetByCodeAsync(string code)
			=> await _context.Achievements.FirstOrDefaultAsync(a => a.Code == code);

		public async Task<List<Achievement>> GetAllAsync()
			=> await _context.Achievements.OrderBy(a => a.Title).ToListAsync();

		public async Task<List<UserAchievement>> GetUserAchievementsAsync(Guid userId)
			=> await _context.UserAchievements
				.Include(ua => ua.Achievement)
				.Where(ua => ua.UserId == userId)
				.OrderByDescending(ua => ua.EarnedAt)
				.ToListAsync();

		public async Task<bool> HasAchievementAsync(Guid userId, string code)
			=> await _context.UserAchievements
				.AnyAsync(ua => ua.UserId == userId && ua.Achievement.Code == code);

		public async Task<UserAchievement> AddUserAchievementAsync(UserAchievement ua)
		{
			try
			{
				_context.UserAchievements.Add(ua);
				await _context.SaveChangesAsync();
				await _context.Entry(ua).Reference(x => x.Achievement).LoadAsync();
				return ua;
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении достижения userId={UserId}", ua.UserId);
				throw;
			}
		}
	}
}
