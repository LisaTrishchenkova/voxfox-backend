using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class CourseRepository : ICourseRepository
{
	private readonly ApplicationContext _context;
	private readonly ILogger<CourseRepository> _logger;

	public CourseRepository(ApplicationContext context, ILogger<CourseRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task<Course> AddAsync(Course course)
	{
		try
		{
			_context.Courses.Add(course);
			await _context.SaveChangesAsync();

			await _context.Entry(course)
				.Reference(c => c.Author) // Reference - для одиночной связи
				.LoadAsync();

			return course;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.StackTrace);
			throw;
		}
	}

	public async Task<bool> DeleteSoftAsync(Course course)
	{
		try
		{
			course.IsDeleted = true;

			_context.Courses.Update(course);
			await _context.SaveChangesAsync();

			return true;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public Task<bool> ExistCourseByIdAsync(Guid id)
	{
		return _context.Courses.AnyAsync(c => c.Id == id);
	}

	public async Task<IList<Course>?> GetAllAsync()
	{
		try
		{
			var courses = await _context.Courses
				.AsNoTracking()
				.Include(c => c.Tags)
				.Include(c => c.Author)
				.ToListAsync();

			return courses;
		}
		catch (OperationCanceledException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<Course?> GetByIdAsync(Guid id)
	{
		try
		{
			var course = await _context.Courses
				.Include(c => c.Tags)
				.Include(c => c.Author)
				.Include(c => c.Reviewer)
				.FirstOrDefaultAsync(c => c.Id == id);

			return course;
		}
		catch (OperationCanceledException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<Course> UpdateAsync(Course course)
	{
		try
		{
			_context.Courses.Update(course);
			await _context.SaveChangesAsync();

			return course;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<IList<Section>> GetSectionsByCourseIdAsync(Guid courseId)
	{
		try
		{
			var courses = await _context.Sections
				.Where(s => s.CourseId == courseId)
				.ToListAsync();

			return courses;
		}
		catch (OperationCanceledException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	//TODO: доделать чтобы было из Jwt
	public async Task<IList<Course>> GetByAuthorIdAsync(Guid authorId, CourseStatus? status = null)
	{
		try
		{
			var query = _context.Courses
				.Include(c => c.Tags)
				.Include(c => c.Author)
				.Where(c => c.AuthorId == authorId);

			if (status.HasValue)
				query = query.Where(c => c.Status == status.Value);

			return await query
				.OrderByDescending(c => c.CreatedAt)
				.ToListAsync();
		}
		catch (System.Exception ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task UpdateEnrollmentCountAsync(Guid courseId)
	{
		try
		{
			var totalCount = await _context.Enrollments
				.CountAsync(e => e.CourseId == courseId);

			await _context.Courses
				.Where(c => c.Id == courseId)
				.ExecuteUpdateAsync(setters => setters
					.SetProperty(c => c.EnrollmentCount, totalCount)
					.SetProperty(c => c.UpdatedAt, DateTime.UtcNow));
		}
		catch (System.Exception ex)
		{
			_logger.LogError(ex, "Ошибка при обновлении EnrollmentCount для курса {CourseId}", courseId);
			throw;
		}
	}

	public IQueryable<Course> GetCoursesQuery()
	{
		return _context.Courses
			// .Include(c => c.Category)
			.AsNoTracking()
			.AsQueryable();
	}

	public IQueryable<Course> GetPublishedCoursesQuery()
	{
		return _context.Courses
			.Where(c => c.Status == CourseStatus.Published)
			.AsNoTracking()
			.Include(c => c.Author)
			.AsQueryable();
	}

	public async Task<List<CourseDto>> GetCoursesWithProjectionAsync(IQueryable<Course> query, int skip, int take)
	{
		return await query
			.Skip(skip)
			.Take(take)
			.Select(c => new CourseDto
			{
				Id = c.Id,
				Title = c.Title,
				Description = c.Description,
				FullDescription = c.FullDescription,
				CoverImageUrl = c.CoverImageUrl,
				Price = c.Price,
				Level = c.Level,
				CertificateEnabled = c.CertificateEnabled,
				EnrollmentCount = c.EnrollmentCount,
				Rating = c.Rating,
				DurationMinutes = c.DurationMinutes,
				Status = c.Status,
				Tags = c.Tags != null
					? c.Tags.Select(t => new TagDto
					{
						Name = t.Name
					}).ToList()
					: new List<TagDto>(),
				CategoryId = c.CategoryId,
				Author = new AuthorDto
				{
					Id = c.Author!.Id,
					Name = c.Author.Name
				},
				PublishedAt = c.PublishedAt,
				CreatedAt = c.CreatedAt,
				ReviewerId = c.ReviewerId,
				ReviewerName = c.Reviewer != null ? c.Reviewer.Name : null,
				ReviewStartedAt = c.ReviewStartedAt,
				ReviewCount = c.ReviewCount
			})
			.ToListAsync();
	}

	public async Task<int> GetTotalCountAsync(IQueryable<Course> query)
	{
		return await query.CountAsync();
	}

	public async Task DeleteTagAsync(Tag tag)
	{
		_context.Tags.Remove(tag);
		await _context.SaveChangesAsync();
	}

	public async Task<List<Course>> GetForReindexAsync(int skip, int take, CancellationToken ct)
	{
		return await _context.Courses
			.Where(c => !c.IsDeleted)
			.OrderBy(c => c.CreatedAt)
			.Skip(skip)
			.Take(take)
			.ToListAsync(ct);
	}

	public IQueryable<Course> GetPendingCoursesQuery()
	{
		var courses = _context.Courses
			.Where(c => c.Status == CourseStatus.UnderReview && !c.IsDeleted)
			.Include(c => c.Author)
			.Include(c => c.Tags)
			.Include(c => c.Reviewer)
			.OrderBy(c => c.UpdatedAt)
			.AsQueryable();

		return courses;
	}
}
