using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces.Task;
using VoxFox.Models.DTOs.Tasks;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class TaskRepository : ITaskRepository
{
	private readonly ApplicationContext _context;
	private readonly ILogger<TaskRepository> _logger;

	public TaskRepository(ApplicationContext context, ILogger<TaskRepository> logger)
	{
		_context = context;
		_logger = logger;
	}


	public async Task<TaskEntity> AddAsync(TaskEntity task)
	{
		try
		{
			_context.Tasks.Add(task);
			await _context.SaveChangesAsync();
			return task;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<TaskEntity?> GetByIdAsync(Guid id)
	{
		var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
		return task;
	}

	public async Task<TaskEntity?> GetByIdWithLessonAsync(Guid id)
	{
		var task = await _context.Tasks
			.Include(t => t.Lesson)
			.FirstOrDefaultAsync(t => t.Id == id);
		return task;
	}

	public async Task<IList<TaskEntity>> GetByLessonIdAsync(Guid lessonId)
	{
		var lessons = await _context.Tasks
			.Where(t => t.LessonId == lessonId)
			.OrderBy(t => t.OrderIndex)
			.ToListAsync();

		return lessons;
	}

	public async Task<int> GetMaxOrderIndexAsync(Guid lessonId)
	{
		var max = await _context.Tasks
			.Where(t => t.LessonId == lessonId)
			.MaxAsync(t => (int?)t.OrderIndex);
		return max ?? 0;
	}

	public async Task<bool> DeleteAsync(TaskEntity task)
	{
		try
		{
			_context.Tasks.Remove(task);
			await _context.SaveChangesAsync();
			return true;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<TaskSubmission> AddSubmissionAsync(TaskSubmission submission)
	{
		try
		{
			_context.TaskSubmissions.Add(submission);
			await _context.SaveChangesAsync();
			return submission;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<TaskSubmission?> GetLastSubmissionAsync(Guid taskId, Guid userId)
	{
		return await _context.TaskSubmissions
			.Where(s => s.TaskId == taskId && s.UserId == userId)
			.OrderByDescending(s => s.AttemptNumber)
			.FirstOrDefaultAsync();
	}

	public async Task<IList<TaskSubmission>> GetSubmissionsByTaskIdAsync(Guid taskId)
	{
		return await _context.TaskSubmissions
			.Where(s => s.TaskId == taskId)
			.OrderByDescending(s => s.SubmittedAt)
			.ToListAsync();
	}
}
