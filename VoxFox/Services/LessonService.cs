using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces.Achievement;
using VoxFox.Interfaces.Certificate;
using VoxFox.Interfaces.Lesson;
using VoxFox.Models;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Services.Course;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ILogger<LessonService> _logger;
    private readonly ILessonProgressRepository _progressRepository;
    private readonly ApplicationContext _context;
    private readonly ICertificateService _certificateService;
    private readonly IAchievementService _achievementService;

    public LessonService(
        ILessonRepository lessonRepository,
        ILogger<LessonService> logger,
        ILessonProgressRepository progressRepository,
        ApplicationContext context,
        ICertificateService certificateService,
        IAchievementService achievementService)
    {
        _lessonRepository = lessonRepository;
        _logger = logger;
        _progressRepository = progressRepository;
        _context = context;
        _certificateService = certificateService;
        _achievementService = achievementService;
    }

    public async Task<ServiceResult<LessonDto>> CreateLessonAsync(Guid sectionId, CreateLessonDto createLessonDto)
    {
        _logger.LogInformation("Creating lesson for section: {SectionId}", sectionId);

        var validationResult = await ValidateLessonAsync(sectionId, createLessonDto);
        if (!validationResult.IsSuccess)
            return ServiceResult<LessonDto>.Fail(validationResult.Error, validationResult.StatusCode);

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
                return ServiceResult<bool>.Fail($"Не удалось удалить урок по id: {id}", StatusCodes.Status500InternalServerError);

            return ServiceResult<bool>.Ok(true, "Урок успешно удален");
        }
        catch (System.Exception ex)
        {
            return ServiceResult<bool>.Fail($"Ошибка при удалении урока: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<ServiceResult<LessonProgressDto>> CompleteLessonAsync(Guid lessonId, Guid userId)
    {
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
            return ServiceResult<LessonProgressDto>.Fail("Урок не найден", 404);

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.UserId == userId &&
                                      e.Course!.Sections.Any(s =>
                                          s.Lessons.Any(l => l.Id == lessonId)));
        if (enrollment == null)
            return ServiceResult<LessonProgressDto>.Fail("Вы не записаны на этот курс", 403);

        if (enrollment.Course == null ||
            enrollment.Course.Status != CourseStatus.Published ||
            enrollment.Course.IsDeleted)
            return ServiceResult<LessonProgressDto>.Fail("Курс недоступен", 403);

        var existing = await _progressRepository.GetAsync(userId, lessonId);
        if (existing != null)
            return ServiceResult<LessonProgressDto>.Fail("Урок уже отмечен как пройденный", 409);

        var progress = new LessonProgress
        {
            UserId = userId,
            LessonId = lessonId,
            EnrollmentId = enrollment.Id,
            CompletedAt = DateTime.UtcNow
        };
        await _progressRepository.AddAsync(progress);

        var completed = await _progressRepository.CountCompletedAsync(enrollment.Id);
        var total = await _progressRepository.CountTotalLessonsInCourseAsync(enrollment.CourseId);

        enrollment.ProgressPercent = total > 0
            ? (int)Math.Round((double)completed / total * 100)
            : 0;

        if (enrollment.ProgressPercent >= 100)
        {
            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // ─── Сертификат ───────────────────────────────────────────
        if (enrollment.ProgressPercent >= 100)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == enrollment.CourseId);
            if (course != null)
                await _certificateService.IssueCertificateAsync(userId, enrollment.CourseId, enrollment.Id, course.CertificateEnabled);
        }

        // ─── Ачивки за уроки ──────────────────────────────────────
        var newAchievements = await _achievementService.CheckAndAwardAsync(userId, AchievementTrigger.LessonCompleted);

        // ─── Ачивки за завершение курса ───────────────────────────
        if (enrollment.ProgressPercent >= 100)
        {
            var courseAchievements = await _achievementService.CheckAndAwardAsync(userId, AchievementTrigger.CourseCompleted);
            newAchievements.AddRange(courseAchievements);
        }

        return ServiceResult<LessonProgressDto>.Ok(new LessonProgressDto
        {
            LessonId = lessonId,
            EnrollmentId = enrollment.Id,
            CompletedAt = progress.CompletedAt,
            ProgressPercent = enrollment.ProgressPercent,
            NewAchievements = newAchievements
        });
    }

    public async Task<ServiceResult<bool>> ReorderLessonsAsync(Guid sectionId, List<Guid> lessonIds)
    {
        var lessons = await _context.Lessons
            .Where(l => l.SectionId == sectionId && lessonIds.Contains(l.Id))
            .ToListAsync();

        if (lessons.Count != lessonIds.Count)
            return ServiceResult<bool>.Fail("Некоторые уроки не найдены или не принадлежат этой секции", 400);

        for (int i = 0; i < lessonIds.Count; i++)
        {
            var lesson = lessons.First(l => l.Id == lessonIds[i]);
            lesson.OrderIndex = i;
        }

        await _context.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<LessonDto?>> GetLessonByIdAsync(Guid id)
    {
        try
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
                return ServiceResult<LessonDto?>.Fail($"Урок с id: {id} не найден", StatusCodes.Status404NotFound);

            return ServiceResult<LessonDto?>.Ok(MapToDo(lesson));
        }
        catch (System.Exception ex)
        {
            return ServiceResult<LessonDto?>.Fail($"Ошибка при получении урока: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<ServiceResult<LessonDto>> UpdateLessonAsync(Guid id, UpdateLessonDto updateLessonDto)
    {
        try
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
                return ServiceResult<LessonDto>.Fail($"Урок с id: {id} не найден", StatusCodes.Status404NotFound);

            lesson.Title = updateLessonDto.Title ?? lesson.Title;
            lesson.Description = updateLessonDto.Description ?? lesson.Description;
            lesson.Content = updateLessonDto.Content ?? lesson.Content;

            var updatedLesson = await _lessonRepository.UpdateAsync(lesson);
            return ServiceResult<LessonDto>.Ok(MapToDo(updatedLesson), "Урок успешно обновлен");
        }
        catch (System.Exception ex)
        {
            return ServiceResult<LessonDto>.Fail($"Ошибка при обновлении урока: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    private async Task<ValidationResult> ValidateLessonAsync(Guid sectionId, CreateLessonDto dto)
    {
        if (sectionId == Guid.Empty)
            return ValidationResult.Fail("sectionId не может быть пустым", StatusCodes.Status400BadRequest);

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
