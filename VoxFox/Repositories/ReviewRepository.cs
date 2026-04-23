using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces.Review;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class ReviewRepository : IReviewRepository
{
	private readonly ApplicationContext _context;
	private readonly ILogger<ReviewRepository> _logger;

	public ReviewRepository(ApplicationContext context, ILogger<ReviewRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task<Review?> GetByIdAsync(Guid id)
	{
		var review = await _context.Reviews
			.Include(r => r.User)
			.FirstOrDefaultAsync(r => r.Id == id);
		return review;
	}

	public async Task<Review?> GetByUserAndCourseAsync(Guid userId, Guid courseId)
	{
		var resualt = await _context.Reviews
			.FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId);
		return resualt;
	}

	public async Task<IList<Review>> GetByCourseIdAsync(Guid courseId)
	{
		var resualt = await _context.Reviews
			.Where(r => r.CourseId == courseId)
			.Include(r => r.User)
			.OrderByDescending(r => r.CreatedAt)
			.ToListAsync();

		return resualt;
	}

	public async Task<Review> AddAsync(Review review)
	{
		try
		{
			_context.Reviews.Add(review);
			await _context.SaveChangesAsync();
			return review;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<Review> UpdateAsync(Review review)
	{
		try
		{
			_context.Reviews.Update(review);
			await _context.SaveChangesAsync();
			return review;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<bool> DeleteAsync(Review review)
	{
		try
		{
			_context.Reviews.Remove(review);
			await _context.SaveChangesAsync();
			return true;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<double> GetAverageRatingAsync(Guid courseId)
	{
		var reviews = await _context.Reviews
			.Where(r => r.CourseId == courseId)
			.ToListAsync();

		return reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating);
	}
}
