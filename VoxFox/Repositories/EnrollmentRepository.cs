using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces.Enrollment;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
	private readonly ApplicationContext _context;
	private readonly ILogger<EnrollmentRepository> _logger;

	public EnrollmentRepository(ApplicationContext context, ILogger<EnrollmentRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task<Enrollment> AddAsync(Enrollment enrollment)
	{
		try
		{
			_context.Enrollments.Add(enrollment);
			await _context.SaveChangesAsync();

			return enrollment;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<Enrollment?> GetByIdAsync(Guid id)
	{
		var enrollment = await _context.Enrollments
			.Include(e => e.Course)
			.FirstOrDefaultAsync(e => e.Id == id);
		return enrollment;
	}

	public async Task<Enrollment?> GetByUserAndCourseAsync(Guid userId, Guid courseId)
	{
		var result = await _context.Enrollments
			.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
		return result;
	}

	public async Task<IList<Enrollment>> GetByUserIdAsync(Guid userId)
	{
		var result = await _context.Enrollments
			.Where(e => e.UserId == userId)
			.Include(e => e.Course)
				.ThenInclude(c => c!.Author)
			.Include(e => e.Course)
				.ThenInclude(c => c!.Tags)
			.ToListAsync();
		return result;
	}

	public async Task<bool> ExistsAsync(Guid userId, Guid courseId)
	{
		var result = await _context.Enrollments
			.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
		return result;
	}

	public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
	{
		try
		{
			_context.Enrollments.Update(enrollment);
			await _context.SaveChangesAsync();
			return enrollment;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<bool> DeleteAsync(Enrollment enrollment)
	{
		try
		{
			_context.Enrollments.Remove(enrollment);
			await _context.SaveChangesAsync();
			return true;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<IList<Enrollment>> GetByCourseIdAsync(Guid courseId)
	{
			var enrollments = await _context.Enrollments
				.Where(e => e.CourseId == courseId)
				.Include(e => e.User)
				.ToListAsync();
			return enrollments;
	}
}
