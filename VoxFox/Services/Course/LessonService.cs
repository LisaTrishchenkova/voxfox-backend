using VoxFox.Interfaces.Lesson;
using VoxFox.Models;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Services.Course;

public class LessonService : ILessonService
{
	private readonly ILessonRepository _lessonRepository;
	private readonly ILogger<LessonService> _logger;

	public LessonService(ILessonRepository lessonRepository, ILogger<LessonService> logger)
	{
		_lessonRepository = lessonRepository;
		_logger = logger;
	}

	public async Task<ServiceResult<LessonDto>> CreateLessonAsync(Guid sectionId, CreateLessonDto createLessonDto)
	{
		_logger.LogInformation("Creating lesson for section: {SectionId}", sectionId);

		// Validation
		var validationResult = await ValidateLessonAsync(sectionId, createLessonDto);
		if (!validationResult.IsSuccess)
			return ServiceResult<LessonDto>.Fail(validationResult.Error, validationResult.StatusCode);

		// Business logic
		var lesson = new Lesson
		{
			Title = createLessonDto.Title,
			Description = createLessonDto.Description,
			Content = createLessonDto.Content,
			SectionId = sectionId
		};

		var createLesson = await _lessonRepository.AddAsync(lesson);

		return ServiceResult<LessonDto>.Created(MapToDo(createLesson));
	}

	public async Task<ServiceResult<bool>> DeleteLessonAsync(Guid id)
	{
		try
		{
			var lesson = await _lessonRepository.GetByIdAsync(id);
			if (lesson == null)
				return ServiceResult<bool>.Fail($"Урок с id: {id} не найден", StatusCodes.Status404NotFound);

			var isSuccess = await _lessonRepository.DeleteSoftAsync(lesson);
			if (!isSuccess)
				return ServiceResult<bool>.Fail($"Не удалось удалить урок по id: {id}",
					StatusCodes.Status500InternalServerError);

			return ServiceResult<bool>.Ok(true, "Урок успешно удален");
		}
		catch (Exception ex)
		{
			return ServiceResult<bool>.Fail(
				$"Ошибка при удалении урока: {ex.Message}",
				StatusCodes.Status500InternalServerError
			);
		}
	}

	public async Task<ServiceResult<LessonDto?>> GetLessonByIdAsync(Guid id)
	{
		try
		{
			var lesson = await _lessonRepository.GetByIdAsync(id);
			if (lesson == null)
				return ServiceResult<LessonDto?>.Fail(
					$"Урок с id: {id} не найден",
					StatusCodes.Status404NotFound
				);

			return ServiceResult<LessonDto?>.Ok(MapToDo(lesson));
		}
		catch (Exception ex)
		{
			return ServiceResult<LessonDto?>.Fail(
				$"Ошибка при получении урока: {ex.Message}",
				StatusCodes.Status500InternalServerError
			);
		}
	}

	public async Task<ServiceResult<LessonDto>> UpdateLessonAsync(Guid id, UpdateLessonDto updateLessonDto)
	{
		try
		{
			var lesson = await _lessonRepository.GetByIdAsync(id);
			if (lesson == null)
				return ServiceResult<LessonDto>.Fail(
					$"Урок с id: {id} не найден",
					StatusCodes.Status404NotFound
				);

			lesson.Title = updateLessonDto.Title ?? lesson.Title;
			lesson.Description = updateLessonDto.Description ?? lesson.Description;
			lesson.Content = updateLessonDto.Content ?? lesson.Content;

			var updatedLesson = await _lessonRepository.UpdateAsync(lesson);

			return ServiceResult<LessonDto>.Ok(MapToDo(updatedLesson), "Урок успешно обновлен");
		}
		catch (Exception ex)
		{
			return ServiceResult<LessonDto>.Fail(
				$"Ошибка при обновлении урока: {ex.Message}",
				StatusCodes.Status500InternalServerError
			);
		}
	}

	private async Task<ValidationResult> ValidateLessonAsync(Guid sectionId, CreateLessonDto dto)
	{
		var section = await _lessonRepository.SectionExistsAsync(sectionId);
		if (!section)
			return ValidationResult.Fail("Section not found", 404);

		if (string.IsNullOrWhiteSpace(dto.Title))
			return ValidationResult.Fail("Title is required");

		if (dto.Title.Length > 200)
			return ValidationResult.Fail("Title too long");

		return ValidationResult.Success();
	}

	private LessonDto MapToDo(Lesson lesson)
	{
		return new LessonDto
		{
			Id = lesson.Id,
			Title = lesson.Title,
			Description = lesson.Description,
			Content = lesson.Content!
		};
	}
}
