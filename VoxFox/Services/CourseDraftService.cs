using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces.DraftCourse;
using VoxFox.Interfaces.Notification;
using VoxFox.Models.DTOs.Draft.CourseDraftDto;
using VoxFox.Models.DTOs.Draft.CreateCourseDraftDto;
using VoxFox.Models.Entities;

using TaskEntity = VoxFox.Models.DTOs.Tasks.TaskEntity;

namespace VoxFox.Services;

public class CourseDraftService : ICourseDraftService
{
    private readonly ApplicationContext _context;
    private readonly ICourseDraftRepository _draftRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CourseDraftService> _logger;


    public CourseDraftService(ApplicationContext context, ICourseDraftRepository draftRepository, INotificationService notificationService, ILogger<CourseDraftService> logger)
    {
        _context = context;
        _draftRepository = draftRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<ServiceResult<CourseDraftDto>> CreateDraftAsync(Guid courseId, Guid authorId)
    {
        var course = await _context.Courses
            .Include(c => c.Tags)
            .Include(c => c.Sections.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Lessons.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return ServiceResult<CourseDraftDto>.Fail("Курс не найден", 404);

        if (course.AuthorId != authorId)
            return ServiceResult<CourseDraftDto>.Fail("Нет доступа к этому курсу", 403);

        if (course.Status != CourseStatus.Published)
            return ServiceResult<CourseDraftDto>.Fail(
                "Черновик можно создать только для опубликованного курса");

        var existing = await _draftRepository.GetByCourseIdAsync(courseId);
        if (existing != null)
            return ServiceResult<CourseDraftDto>.Fail(
                "У этого курса уже есть активный черновик", 409);

        var now = DateTime.UtcNow;

        var draft = new CourseDraft
        {
            CourseId = courseId,
            AuthorId = authorId,
            Title = course.Title,
            Description = course.Description,
            FullDescription = course.FullDescription,
            CoverImageUrl = course.CoverImageUrl,
            Price = course.Price,
            Level = course.Level,
            CertificateEnabled = course.CertificateEnabled,
            CategoryId = course.CategoryId,
            Status = DraftStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
            Tags = course.Tags?.Select(t => new DraftTag { Name = t.Name }).ToList() ?? [],
        };

        foreach (var section in course.Sections.OrderBy(s => s.OrderIndex))
        {
            var draftSection = new DraftSection
            {
                OriginalSectionId = section.Id,
                Title = section.Title,
                Description = section.Description,
                OrderIndex = section.OrderIndex,
            };

            foreach (var lesson in section.Lessons.OrderBy(l => l.OrderIndex))
            {
                var draftLesson = new DraftLesson
                {
                    OriginalLessonId = lesson.Id,
                    Title = lesson.Title,
                    Description = lesson.Description,
                    Content = lesson.Content,
                    OrderIndex = lesson.OrderIndex,
                };

                var tasks = await _context.Tasks
                    .AsQueryable()
                    .Where(t => t.LessonId == lesson.Id && !t.IsDeleted)
                    .OrderBy(t => t.OrderIndex)
                    .ToListAsync();

                foreach (var task in tasks)
                {
                    draftLesson.Tasks.Add(new DraftTask
                    {
                        OriginalTaskId = task.Id,
                        Type = task.Type,
                        Question = task.Question,
                        Options = task.Options,
                        CorrectIndex = task.CorrectIndex,
                        CorrectIndexes = task.CorrectIndexes,
                        CorrectAnswer = task.CorrectAnswer,
                        Explanation = task.Explanation,
                        Points = task.Points,
                        IsRequired = task.IsRequired,
                        OrderIndex = task.OrderIndex,
                    });
                }

                draftSection.Lessons.Add(draftLesson);
            }

            draft.Sections.Add(draftSection);
        }

        await _draftRepository.AddAsync(draft);

        return ServiceResult<CourseDraftDto>.Ok(MapToDto(draft));
    }

    public async Task<ServiceResult<CourseDraftDto>> GetDraftAsync(Guid courseId, Guid authorId)
    {
        var draft = await _draftRepository.GetByCourseIdAsync(courseId);

        if (draft == null)
            return ServiceResult<CourseDraftDto>.Fail("Черновик не найден", 404);

        if (draft.AuthorId != authorId)
            return ServiceResult<CourseDraftDto>.Fail("Нет доступа", 403);

        return ServiceResult<CourseDraftDto>.Ok(MapToDto(draft));
    }

    public async Task<ServiceResult<CourseDraftDto>> UpdateDraftAsync(
        Guid draftId, CreateCourseDraftDto dto, Guid authorId)
    {
        var draft = await _draftRepository.GetByIdAsync(draftId);

        if (draft == null)
            return ServiceResult<CourseDraftDto>.Fail("Черновик не найден", 404);

        if (draft.AuthorId != authorId)
            return ServiceResult<CourseDraftDto>.Fail("Нет доступа", 403);

        if (draft.Status == DraftStatus.UnderReview)
            return ServiceResult<CourseDraftDto>.Fail(
                "Нельзя редактировать черновик пока он на проверке");

        draft.Title = dto.Title;
        draft.Description = dto.Description;
        draft.FullDescription = dto.FullDescription;
        draft.CoverImageUrl = dto.CoverImageUrl;
        draft.Price = dto.Price;
        draft.Level = dto.Level;
        draft.CertificateEnabled = dto.CertificateEnabled;
        draft.CategoryId = dto.CategoryId == Guid.Empty ? null : dto.CategoryId;
        draft.Status = DraftStatus.Draft;
        draft.UpdatedAt = DateTime.UtcNow;

        _context.DraftTags.RemoveRange(draft.Tags);
        draft.Tags = dto.Tags.Select(t => new DraftTag
        {
            DraftId = draft.Id,
            Name = t
        }).ToList();

        _context.DraftSections.RemoveRange(draft.Sections);

        foreach (var sDto in dto.Sections.OrderBy(s => s.OrderIndex))
        {
            var section = new DraftSection
            {
                DraftId = draft.Id,
                OriginalSectionId = sDto.OriginalSectionId,
                Title = sDto.Title,
                Description = sDto.Description,
                OrderIndex = sDto.OrderIndex,
            };

            foreach (var lDto in sDto.Lessons.OrderBy(l => l.OrderIndex))
            {
                var lesson = new DraftLesson
                {
                    OriginalLessonId = lDto.OriginalLessonId,
                    Title = lDto.Title,
                    Description = lDto.Description,
                    Content = lDto.Content,
                    OrderIndex = lDto.OrderIndex,
                };

                foreach (var tDto in lDto.Tasks.OrderBy(t => t.OrderIndex))
                {
                    if (!Enum.TryParse<TaskType>(tDto.Type, out var taskType))
                        continue;

                    lesson.Tasks.Add(new DraftTask
                    {
                        OriginalTaskId = tDto.OriginalTaskId,
                        Type = taskType,
                        Question = tDto.Question,
                        Options = tDto.Options,
                        CorrectIndex = tDto.CorrectIndex,
                        CorrectIndexes = tDto.CorrectIndexes,
                        CorrectAnswer = tDto.CorrectAnswer,
                        Explanation = tDto.Explanation,
                        Points = tDto.Points,
                        IsRequired = tDto.IsRequired,
                        OrderIndex = tDto.OrderIndex,
                    });
                }

                section.Lessons.Add(lesson);
            }

            draft.Sections.Add(section);
        }

        await _draftRepository.UpdateAsync(draft);

        return ServiceResult<CourseDraftDto>.Ok(MapToDto(draft));
    }

    public async Task<ServiceResult<bool>> SubmitDraftAsync(Guid draftId, Guid authorId)
    {
        var draft = await _draftRepository.GetByIdAsync(draftId);

        if (draft == null)
            return ServiceResult<bool>.Fail("Черновик не найден", 404);

        if (draft.AuthorId != authorId)
            return ServiceResult<bool>.Fail("Нет доступа", 403);

        if (draft.Status != DraftStatus.Draft && draft.Status != DraftStatus.RejectedByModerator)
            return ServiceResult<bool>.Fail("Черновик уже отправлен на проверку");

        draft.Status = DraftStatus.UnderReview;
        draft.UpdatedAt = DateTime.UtcNow;

        await _draftRepository.UpdateAsync(draft);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> ApproveDraftAsync(Guid draftId)
    {
        var draft = await _draftRepository.GetByIdAsync(draftId);

        if (draft == null)
            return ServiceResult<bool>.Fail("Черновик не найден", 404);

        if (draft.Status != DraftStatus.UnderReview)
            return ServiceResult<bool>.Fail("Черновик не находится на проверке");

        var course = await _context.Courses
            .Include(c => c.Tags)
            .Include(c => c.Sections.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Lessons.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == draft.CourseId);

        if (course == null)
            return ServiceResult<bool>.Fail("Курс не найден", 404);

        var now = DateTime.UtcNow;

        course.Title = draft.Title;
        course.Description = draft.Description;
        course.FullDescription = draft.FullDescription;
        course.CoverImageUrl = draft.CoverImageUrl;
        course.Price = draft.Price;
        course.Level = draft.Level;
        course.CertificateEnabled = draft.CertificateEnabled;
        course.CategoryId = draft.CategoryId == Guid.Empty ? null : draft.CategoryId;
        course.UpdatedAt = now;

        if (course.Tags != null)
            foreach (var tag in course.Tags.ToList())
                _context.Tags.Remove(tag);

        course.Tags = draft.Tags.Select(t => new Tag
        {
            CourseId = course.Id,
            Name = t.Name
        }).ToList();

        await ApplyDraftStructureAsync(course, draft, now);

        await _draftRepository.DeleteAsync(draft);

        await _notificationService.SendAsync(
            draft.AuthorId,
            "Изменения одобрены",
            $"Обновления курса «{course.Title}» прошли модерацию и применены",
            NotificationType.CourseApproved,
            relatedEntityId: course.Id,
            relatedCourseId: course.Id);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> RejectDraftAsync(Guid draftId, string? reason)
    {
        var draft = await _draftRepository.GetByIdAsync(draftId);

        if (draft == null)
            return ServiceResult<bool>.Fail("Черновик не найден", 404);

        if (draft.Status != DraftStatus.UnderReview)
            return ServiceResult<bool>.Fail("Черновик не находится на проверке");

        draft.Status = DraftStatus.RejectedByModerator;
        draft.UpdatedAt = DateTime.UtcNow;

        await _draftRepository.UpdateAsync(draft);

        await _notificationService.SendAsync(
            draft.AuthorId,
            "Изменения отклонены",
            $"Обновления курса «{draft.Title}» отклонены модератором." +
            (string.IsNullOrWhiteSpace(reason) ? "" : $" Причина: {reason}"),
            NotificationType.CourseRejected,
            relatedEntityId: draft.CourseId,
            relatedCourseId: draft.CourseId);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteDraftAsync(Guid draftId, Guid authorId)
    {
        var draft = await _draftRepository.GetByIdAsync(draftId);

        if (draft == null)
            return ServiceResult<bool>.Fail("Черновик не найден", 404);

        if (draft.AuthorId != authorId)
            return ServiceResult<bool>.Fail("Нет доступа", 403);

        if (draft.Status == DraftStatus.UnderReview)
            return ServiceResult<bool>.Fail("Нельзя удалить черновик пока он на проверке");

        await _draftRepository.DeleteAsync(draft);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<CourseDraftDto>> GetDraftForReviewAsync(Guid draftId)
    {
        var draft = await _draftRepository.GetByIdAsync(draftId);

        if (draft == null)
            return ServiceResult<CourseDraftDto>.Fail("Черновик не найден", 404);

        return ServiceResult<CourseDraftDto>.Ok(MapToDto(draft));
    }

    public async Task<List<CourseDraftDto>> GetPendingDraftsAsync()
    {
        var drafts = await _draftRepository.GetPendingAsync();
        return drafts.Select(MapToDto).ToList();
    }

    // ─── Применение структуры черновика к курсу ─────────────

    private async Task ApplyDraftStructureAsync(Models.Entities.Course course, CourseDraft draft, DateTime now)
    {
        try
        {
            var existingSections = course.Sections.ToList();
            var existingLessons = existingSections.SelectMany(s => s.Lessons).ToList();
            var existingLessonIds = existingLessons.Select(l => l.Id).ToList();

            var existingTasks = await _context.Tasks
                .AsQueryable()
                .Where(t => existingLessonIds.Contains(t.LessonId) && !t.IsDeleted)
                .ToListAsync();

            var draftOriginalSectionIds = draft.Sections
                .Where(s => s.OriginalSectionId.HasValue)
                .Select(s => s.OriginalSectionId!.Value)
                .ToHashSet();

            var draftOriginalLessonIds = draft.Sections
                .SelectMany(s => s.Lessons)
                .Where(l => l.OriginalLessonId.HasValue)
                .Select(l => l.OriginalLessonId!.Value)
                .ToHashSet();

            var draftOriginalTaskIds = draft.Sections
                .SelectMany(s => s.Lessons)
                .SelectMany(l => l.Tasks)
                .Where(t => t.OriginalTaskId.HasValue)
                .Select(t => t.OriginalTaskId!.Value)
                .ToHashSet();

            foreach (var section in existingSections.Where(s => !draftOriginalSectionIds.Contains(s.Id)))
                section.IsDeleted = true;

            foreach (var lesson in existingLessons.Where(l => !draftOriginalLessonIds.Contains(l.Id)))
                lesson.IsDeleted = true;

            foreach (var task in existingTasks.Where(t => !draftOriginalTaskIds.Contains(t.Id)))
                task.IsDeleted = true;

            foreach (var draftSection in draft.Sections)
            {
                Section section;

                if (draftSection.OriginalSectionId.HasValue)
                {
                    section = existingSections.FirstOrDefault(s => s.Id == draftSection.OriginalSectionId.Value);
                    if (section == null)
                    {
                        // секция удалена или не найдена — создаём новую
                        section = new Section
                        {
                            CourseId = course.Id,
                            Title = draftSection.Title,
                            Description = draftSection.Description,
                            OrderIndex = draftSection.OrderIndex,
                        };
                        _context.Sections.Add(section);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        section.Title = draftSection.Title;
                        section.Description = draftSection.Description;
                        section.OrderIndex = draftSection.OrderIndex;
                        section.IsDeleted = false;
                    }

                }
                else
                {
                    section = new Section
                    {
                        CourseId = course.Id,
                        Title = draftSection.Title,
                        Description = draftSection.Description,
                        OrderIndex = draftSection.OrderIndex,
                    };
                    _context.Sections.Add(section);
                    await _context.SaveChangesAsync();
                }

                foreach (var draftLesson in draftSection.Lessons)
                {
                    Lesson lesson;

                    if (draftLesson.OriginalLessonId.HasValue)
                    {
                        lesson = existingLessons.FirstOrDefault(l => l.Id == draftLesson.OriginalLessonId.Value);
                        if (lesson == null)
                        {
                            lesson = new Lesson
                            {
                                SectionId = section.Id,
                                Title = draftLesson.Title,
                                Description = draftLesson.Description,
                                Content = draftLesson.Content ?? string.Empty,
                                OrderIndex = draftLesson.OrderIndex,
                            };
                            _context.Lessons.Add(lesson);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            lesson.Title = draftLesson.Title;
                            lesson.Description = draftLesson.Description;
                            lesson.Content = draftLesson.Content ?? string.Empty;
                            lesson.OrderIndex = draftLesson.OrderIndex;
                            lesson.IsDeleted = false;
                        }
                    }
                    else
                    {
                        lesson = new Lesson
                        {
                            SectionId = section.Id,
                            Title = draftLesson.Title,
                            Description = draftLesson.Description,
                            Content = draftLesson.Content ?? string.Empty,
                            OrderIndex = draftLesson.OrderIndex,
                        };
                        _context.Lessons.Add(lesson);
                        await _context.SaveChangesAsync();
                    }

                    foreach (var draftTask in draftLesson.Tasks)
                    {
                        if (draftTask.OriginalTaskId.HasValue)
                        {
                            var task = existingTasks
                                .FirstOrDefault(t => t.Id == draftTask.OriginalTaskId.Value);

                            if (task == null) continue;

                            bool changed = task.Question != draftTask.Question ||
                                           task.CorrectAnswer != draftTask.CorrectAnswer ||
                                           task.CorrectIndex != draftTask.CorrectIndex;

                            task.Type = draftTask.Type;
                            task.Question = draftTask.Question;
                            task.Options = draftTask.Options;
                            task.CorrectIndex = draftTask.CorrectIndex;
                            task.CorrectIndexes = draftTask.CorrectIndexes;
                            task.CorrectAnswer = draftTask.CorrectAnswer;
                            task.Explanation = draftTask.Explanation;
                            task.Points = draftTask.Points;
                            task.IsRequired = draftTask.IsRequired;
                            task.OrderIndex = draftTask.OrderIndex;
                            task.IsDeleted = false;

                            if (changed)
                            {
                                var submissions = await _context.TaskSubmissions
                                    .AsQueryable()
                                    .Where(s => s.TaskId == task.Id)
                                    .ToListAsync();
                                _context.TaskSubmissions.RemoveRange(submissions);
                            }
                        }
                        else
                        {
                            _context.Tasks.Add(new TaskEntity
                            {
                                LessonId = lesson.Id,
                                Type = draftTask.Type,
                                Question = draftTask.Question,
                                Options = draftTask.Options,
                                CorrectIndex = draftTask.CorrectIndex,
                                CorrectIndexes = draftTask.CorrectIndexes,
                                CorrectAnswer = draftTask.CorrectAnswer,
                                Explanation = draftTask.Explanation,
                                Points = draftTask.Points,
                                IsRequired = draftTask.IsRequired,
                                OrderIndex = draftTask.OrderIndex,
                                CreatedAt = now,
                            });
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DbUpdateException: {Message}", ex.InnerException?.Message ?? ex.Message);
            throw;
        }
}

    // ─── Маппинг ─────────────────────────────────────────────

    private static CourseDraftDto MapToDto(CourseDraft draft) => new()
    {
        Id = draft.Id,
        CourseId = draft.CourseId,
        Title = draft.Title,
        Description = draft.Description,
        FullDescription = draft.FullDescription,
        CoverImageUrl = draft.CoverImageUrl,
        Price = draft.Price,
        Level = draft.Level.ToString(),
        CertificateEnabled = draft.CertificateEnabled,
        CategoryId = draft.CategoryId,
        Status = draft.Status.ToString(),
        CreatedAt = draft.CreatedAt,
        UpdatedAt = draft.UpdatedAt,
        Tags = draft.Tags.Select(t => t.Name).ToList(),
        Sections = draft.Sections.OrderBy(s => s.OrderIndex).Select(s => new DraftSectionDto
        {
            Id = s.Id,
            OriginalSectionId = s.OriginalSectionId,
            Title = s.Title,
            Description = s.Description,
            OrderIndex = s.OrderIndex,
            Lessons = s.Lessons.OrderBy(l => l.OrderIndex).Select(l => new DraftLessonDto
            {
                Id = l.Id,
                OriginalLessonId = l.OriginalLessonId,
                Title = l.Title,
                Description = l.Description,
                Content = l.Content,
                OrderIndex = l.OrderIndex,
                Tasks = l.Tasks.OrderBy(t => t.OrderIndex).Select(t => new DraftTaskDto
                {
                    Id = t.Id,
                    OriginalTaskId = t.OriginalTaskId,
                    Type = t.Type.ToString(),
                    Question = t.Question,
                    Options = t.Options,
                    CorrectIndex = t.CorrectIndex,
                    CorrectIndexes = t.CorrectIndexes,
                    CorrectAnswer = t.CorrectAnswer,
                    Explanation = t.Explanation,
                    Points = t.Points,
                    IsRequired = t.IsRequired,
                    OrderIndex = t.OrderIndex,
                }).ToList(),
            }).ToList(),
        }).ToList(),
    };
}
