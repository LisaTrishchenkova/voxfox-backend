using VoxFox.Interfaces.Lesson;
using VoxFox.Interfaces.Question;
using VoxFox.Models.DTOs.Question;
using VoxFox.Models.Entities;

namespace VoxFox.Services;

public class QuestionService : IQuestionService
{
	private readonly IQuestionRepository _questionRepository;
	private readonly ILessonRepository _lessonRepository;
	private readonly ILogger<QuestionService> _logger;

	public QuestionService(
		IQuestionRepository questionRepository,
		ILessonRepository lessonRepository,
		ILogger<QuestionService> logger)
	{
		_questionRepository = questionRepository;
		_lessonRepository = lessonRepository;
		_logger = logger;
	}

	public async Task<ServiceResult<QuestionDto>> CreateQuestionAsync(Guid lessonId, Guid userId, CreateQuestionDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.Text))
			return ServiceResult<QuestionDto>.Fail(
				"Текст вопроса не может быть пустым",
				StatusCodes.Status400BadRequest);

		var lesson = await _lessonRepository.GetByIdAsync(lessonId);
		if (lesson == null)
			return ServiceResult<QuestionDto>.Fail(
				$"Урок с id {lessonId} не найден",
				StatusCodes.Status404NotFound);

		var question = new Question
		{
			LessonId = lessonId,
			AuthorId = userId,
			Text = dto.Text.Trim(),
			CreatedAt = DateTime.UtcNow
		};

		var created = await _questionRepository.AddAsync(question);
		var full = await _questionRepository.GetByIdAsync(created.Id);
		return ServiceResult<QuestionDto>.Ok(MapToDto(full!));
	}

	public async Task<ServiceResult<IList<QuestionDto>>> GetLessonQuestionsAsync(Guid lessonId)
	{
		var lesson = await _lessonRepository.GetByIdAsync(lessonId);
		if (lesson == null)
			return ServiceResult<IList<QuestionDto>>.Fail(
				$"Урок с id: {lessonId} не найден",
				StatusCodes.Status404NotFound);

		var questions = await _questionRepository.GetByLessonIdAsync(lessonId);
		return ServiceResult<IList<QuestionDto>>.Ok(
			questions.Select(MapToDto).ToList());
	}

	public async Task<ServiceResult<QuestionDto>> AnswerQuestionAsync(Guid questionId, Guid userId, AnswerQuestionDto dto)
	{
		if (string.IsNullOrWhiteSpace(dto.AnswerText))
			return ServiceResult<QuestionDto>.Fail(
				"Текст ответа не может быть пустым",
				StatusCodes.Status400BadRequest);

		var question = await _questionRepository.GetByIdAsync(questionId);
		if (question == null)
			return ServiceResult<QuestionDto>.Fail(
				$"Вопрос с id {questionId} не найден",
				StatusCodes.Status404NotFound);

		question.AnswerText = dto.AnswerText.Trim();
		question.AnsweredById = userId;
		question.AnsweredAt = DateTime.UtcNow;

		var updated = await _questionRepository.UpdateAsync(question);
		var full = await _questionRepository.GetByIdAsync(updated.Id);
		return ServiceResult<QuestionDto>.Ok(MapToDto(full!));
	}

	public async Task<ServiceResult<bool>> DeleteQuestionAsync(Guid questionId, Guid userId, string userRole)
	{
		var question = await _questionRepository.GetByIdAsync(questionId);
		if (question == null)
			return ServiceResult<bool>.Fail(
				$"Вопрос с id {questionId} не найден",
				StatusCodes.Status404NotFound);

		var isAdminOrModerator = userRole is "Admin" or "Moderator";
		if (!isAdminOrModerator && question.AuthorId != userId)
			return ServiceResult<bool>.Fail(
				"Нет доступа — это не ваш вопрос",
				StatusCodes.Status403Forbidden);

		await _questionRepository.DeleteAsync(question);
		return ServiceResult<bool>.Ok(true);
	}

	private static QuestionDto MapToDto(Question q) => new()
	{
		Id = q.Id,
		LessonId = q.LessonId,
		AuthorId = q.AuthorId,
		AuthorName = q.Author?.Name,
		Text = q.Text,
		AnswerText = q.AnswerText,
		AnsweredByName = q.AnsweredBy?.Name,
		AnsweredAt = q.AnsweredAt,
		CreatedAt = q.CreatedAt
	};
}
