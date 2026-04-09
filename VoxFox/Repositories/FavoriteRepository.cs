using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
	private readonly ApplicationContext _context;
	private readonly ILogger<EnrollmentRepository> _logger;

	public FavoriteRepository(ApplicationContext context, ILogger<EnrollmentRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task<IList<Favorite>> GetByUserIdAsync(Guid userId)
	{
		var result = await _context.Favorites
			.Include(f => f.Course)
			.ThenInclude(c => c.Author)
			.Include(f => f.Course)
			.ThenInclude(c => c.Tags)
			.Where(f => f.UserId == userId)
			.ToListAsync();
		return result;
	}

	public async Task<Favorite?> GetByUserAndCourseAsync(Guid userId, Guid courseId)
	{
		var result = await _context.Favorites
			.FirstOrDefaultAsync(f => f.UserId == userId && f.CourseId == courseId)!;
		return result;
	}

	public async Task<Favorite> AddAsync(Favorite favorite)
	{
		_context.Favorites.Add(favorite);
		await _context.SaveChangesAsync();
		return favorite;
	}

	public async Task<bool> DeleteAsync(Favorite favorite)
	{
		try
		{
			_context.Favorites.Remove(favorite);
			await _context.SaveChangesAsync();
			return true;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}

	}
}
