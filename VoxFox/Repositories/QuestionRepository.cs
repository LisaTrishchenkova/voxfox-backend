using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces.Question;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class QuestionRepository : IQuestionRepository
{
	private readonly ApplicationContext _context;
	private readonly ILogger<QuestionRepository> _logger;

	public QuestionRepository(ApplicationContext context, ILogger<QuestionRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task<Question?> GetByIdAsync(Guid id)
	{
		var question = await _context.Questions
			.Include(q => q.Author)
			.Include(q => q.AnsweredBy)
			.FirstOrDefaultAsync(q => q.Id == id);

		return question;
	}

	public async Task<IList<Question>> GetByLessonIdAsync(Guid lessonId)
	{
		var lesson = await _context.Questions
			.Where(q => q.LessonId == lessonId)
			.Include(q => q.Author)
			.Include(q => q.AnsweredBy)
			.OrderByDescending(q => q.CreatedAt)
			.ToListAsync();

		return lesson;
	}

	public async Task<Question> AddAsync(Question question)
	{
		try
		{
			_context.Questions.Add(question);
			await _context.SaveChangesAsync();
			return question;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<Question> UpdateAsync(Question question)
	{
		try
		{
			_context.Questions.Update(question);
			await _context.SaveChangesAsync();
			return question;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.Message);
			throw;
		}
	}

	public async Task<bool> DeleteAsync(Question question)
	{
		try
		{
			question.IsDeleted = true;
			_context.Questions.Update(question);
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
