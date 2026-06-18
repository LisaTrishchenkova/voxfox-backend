using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces.DraftCourse;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class CourseDraftRepository : ICourseDraftRepository
{
	private readonly ApplicationContext _context;

	public CourseDraftRepository(ApplicationContext context)
	{
		_context = context;
	}

	public async Task<CourseDraft?> GetByIdAsync(Guid id) =>
		await WithIncludes().FirstOrDefaultAsync(d => d.Id == id);

	public async Task<CourseDraft?> GetByCourseIdAsync(Guid courseId) =>
		await WithIncludes().FirstOrDefaultAsync(d => d.CourseId == courseId);

	public async Task<List<CourseDraft>> GetPendingAsync() =>
		await WithIncludes()
			.Where(d => d.Status == DraftStatus.UnderReview)
			.OrderBy(d => d.UpdatedAt)
			.ToListAsync();

	public async Task<CourseDraft> AddAsync(CourseDraft draft)
	{
		_context.CourseDrafts.Add(draft);
		await _context.SaveChangesAsync();
		return draft;
	}

	public async Task UpdateAsync(CourseDraft draft)
	{
		_context.CourseDrafts.Update(draft);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteAsync(CourseDraft draft)
	{
		_context.CourseDrafts.Remove(draft);
		await _context.SaveChangesAsync();
	}

	private IQueryable<CourseDraft> WithIncludes() =>
		_context.CourseDrafts
			.Include(d => d.Tags)
			.Include(d => d.Reviewer)   // ← добавлено
			.Include(d => d.Sections.OrderBy(s => s.OrderIndex))
			.ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
			.ThenInclude(l => l.Tasks.OrderBy(t => t.OrderIndex));
}
