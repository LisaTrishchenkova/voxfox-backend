using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces;
using VoxFox.Interfaces.Notification;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;
using VoxFox.Models.Requests;
using VoxFox.Models.Responses;

namespace VoxFox.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CourseService> _logger;
    private readonly ApplicationContext _context;

    public CourseService(ICourseRepository courseRepository, INotificationService notificationService, ILogger<CourseService> logger, ApplicationContext context)
    {
        _courseRepository = courseRepository;
        _notificationService = notificationService;
        _logger = logger;
        _context = context;
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto, Guid authorId)
    {
        if (createCourseDto.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == createCourseDto.CategoryId.Value);
            if (!categoryExists)
                throw new System.Exception($"Категория с Id: {createCourseDto.CategoryId} не найдена");
        }
        var author = await _context.Users.FirstOrDefaultAsync(u => u.Id == authorId);
        if (author == null)
            throw new System.Exception("Пользователь не найден");

        var course = new Models.Entities.Course
        {
            Status = CourseStatus.Draft,
            Title = createCourseDto.Title,
            Description = createCourseDto.Description,
            FullDescription = createCourseDto.FullDescription,
            CoverImageUrl = createCourseDto.CoverImageUrl,
            Price = createCourseDto.Price,
            Level = createCourseDto.Level,
            CertificateEnabled = createCourseDto.CertificateEnabled,
            CategoryId = createCourseDto.CategoryId,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = null,
            Tags = createCourseDto.Tags.Select(tagDto => new Tag { Name = tagDto.Name }).ToList()
        };

        var createdCourse = await _courseRepository.AddAsync(course);
        if (createdCourse == null)
            throw new System.Exception("Не удалось добавить курс");

        return MapToDo(createdCourse);
    }

    public async Task<bool> DeleteCourseAsync(Guid id, Guid userId, bool isAdmin = false, string? reason = null)
    {
        _logger.LogInformation("DeleteCourse: id={Id}, userId={UserId}, isAdmin={IsAdmin}, reason={Reason}", id, userId, isAdmin, reason);

        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
        {
            _logger.LogWarning("DeleteCourse: курс не найден id={Id}", id);
            return false;
        }

        if (!isAdmin && course.AuthorId != userId)
        {
            _logger.LogWarning("DeleteCourse: нет доступа. AuthorId={AuthorId}, UserId={UserId}", course.AuthorId, userId);
            return false;
        }

        var isSuccess = await _courseRepository.DeleteSoftAsync(course);
        _logger.LogInformation("DeleteCourse: DeleteSoftAsync result={IsSuccess}", isSuccess);

        _logger.LogInformation("DeleteCourse: isAdmin={IsAdmin}, AuthorId={AuthorId}, userId={UserId}, AuthorId.Value != userId: {Diff}",
            isAdmin, course.AuthorId, userId, course.AuthorId.HasValue && course.AuthorId.Value != userId);

        if (isSuccess && isAdmin && course.AuthorId.HasValue && course.AuthorId.Value != userId)
        {
            _logger.LogInformation("DeleteCourse: отправляем уведомление автору {AuthorId}, reason={Reason}", course.AuthorId.Value, reason);
            await _notificationService.SendAsync(
                course.AuthorId.Value,
                "Курс удалён администратором",
                $"Ваш курс «{course.Title}» был удалён администратором.{(string.IsNullOrWhiteSpace(reason) ? "" : $" Причина: {reason}")}",
                NotificationType.CourseRejected,
                relatedEntityId: course.Id,
                relatedCourseId: course.Id);
        }

        return isSuccess;
    }

    public async Task<ServiceResult<bool>> RestoreCourseAsync(Guid id)
    {
        var course = await _context.Courses
            .IgnoreQueryFilters()
            .Include(c => c.Author)
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return ServiceResult<bool>.Fail("Курс не найден", StatusCodes.Status404NotFound);

        if (!course.IsDeleted)
            return ServiceResult<bool>.Fail("Курс не удалён", StatusCodes.Status400BadRequest);

        course.IsDeleted = false;
        course.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (course.AuthorId.HasValue)
        {
            await _notificationService.SendAsync(
                course.AuthorId.Value,
                "Курс восстановлен",
                $"Ваш курс «{course.Title}» был восстановлен администратором.",
                NotificationType.CourseApproved,
                relatedEntityId: course.Id,
                relatedCourseId: course.Id);
        }

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> UnpublishCourseAsync(Guid id, string? reason = null)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return ServiceResult<bool>.Fail("Курс не найден", StatusCodes.Status404NotFound);

        if (course.Status != CourseStatus.Published)
            return ServiceResult<bool>.Fail("Снять с публикации можно только опубликованный курс");

        course.Status = CourseStatus.Draft;
        course.PublishedAt = null;
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        if (course.AuthorId.HasValue)
        {
            await _notificationService.SendAsync(
                course.AuthorId.Value,
                "Курс снят с публикации",
                $"Ваш курс «{course.Title}» был снят с публикации администратором.{(string.IsNullOrWhiteSpace(reason) ? "" : $" Причина: {reason}")}",
                NotificationType.CourseRejected,
                relatedEntityId: course.Id,
                relatedCourseId: course.Id);
        }

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<IReadOnlyCollection<CourseDto>> GetAllCoursesAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        if (courses == null)
            throw new System.Exception("Не удалось получить список курсов");
        return courses.Select(MapToDo).ToList();
    }

    public async Task<CourseDto?> GetCourseByIdAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return null;
        return MapToDo(course);
    }

    public async Task<ServiceResult<CourseDto>> UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto, Guid userId)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return ServiceResult<CourseDto>.Fail($"Курс с id: {id} не найден", StatusCodes.Status404NotFound);

        if (course.AuthorId != userId)
            return ServiceResult<CourseDto>.Fail("Нет доступа к этому курсу", StatusCodes.Status403Forbidden);

        if (updateCourseDto.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateCourseDto.CategoryId.Value);
            if (!categoryExists)
                return ServiceResult<CourseDto>.Fail($"Категория с Id: {updateCourseDto.CategoryId} не найдена", StatusCodes.Status404NotFound);
        }

        course.Title = updateCourseDto.Title ?? course.Title;
        course.Description = updateCourseDto.Description ?? course.Description;
        course.FullDescription = updateCourseDto.FullDescription ?? course.FullDescription;
        course.CoverImageUrl = updateCourseDto.CoverImageUrl ?? course.CoverImageUrl;
        course.Price = updateCourseDto.Price ?? course.Price;
        course.Level = updateCourseDto.Level ?? course.Level;
        course.CertificateEnabled = updateCourseDto.CertificateEnabled ?? course.CertificateEnabled;
        course.CategoryId = updateCourseDto.CategoryId ?? course.CategoryId;
        course.UpdatedAt = DateTime.UtcNow;

        if (course.Tags != null)
        {
            var existingTags = course.Tags.ToList();
            var newTagNames = updateCourseDto.Tags.Select(t => t.Name).ToList();
            foreach (var existingTag in existingTags)
            {
                if (!newTagNames.Contains(existingTag.Name))
                    await _courseRepository.DeleteTagAsync(existingTag);
            }
            foreach (var tagDto in updateCourseDto.Tags)
            {
                var existingTag = course.Tags.FirstOrDefault(t => t.Name == tagDto.Name);
                if (existingTag == null)
                    course.Tags.Add(new Tag { Name = tagDto.Name, CourseId = course.Id });
            }
        }

        var updateCourse = await _courseRepository.UpdateAsync(course);
        return ServiceResult<CourseDto>.Ok(MapToDo(updateCourse));
    }

    public async Task<ServiceResult<IList<SectionDto>>> GetSectionsByCourseIdAsync(Guid courseId)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                return ServiceResult<IList<SectionDto>>.Fail($"Курс с id: {courseId} не найден", StatusCodes.Status404NotFound);

            var sections = await _courseRepository.GetSectionsByCourseIdAsync(courseId);
            var sectionsDto = sections.Select(MapToDo).ToList();
            return ServiceResult<IList<SectionDto>>.Ok(sectionsDto);
        }
        catch (System.Exception ex)
        {
            return ServiceResult<IList<SectionDto>>.Fail($"Ошибка при получении разделов курса: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    private CourseDto MapToDo(Models.Entities.Course course)
    {
        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            FullDescription = course.FullDescription,
            CoverImageUrl = course.CoverImageUrl,
            Price = course.Price,
            Level = course.Level,
            CertificateEnabled = course.CertificateEnabled,
            EnrollmentCount = course.EnrollmentCount,
            Rating = course.Rating,
            DurationMinutes = course.DurationMinutes,
            Status = course.Status,
            CategoryId = course.CategoryId,
            IsDeleted = course.IsDeleted,
            Tags = course.Tags?.Select(t => new TagDto { Name = t.Name }).ToList(),
            Author = new AuthorDto { Id = course.Author!.Id, Name = course.Author.Name },
            PublishedAt = course.PublishedAt,
            CreatedAt = course.CreatedAt
        };
    }

    private SectionDto MapToDo(Section section)
    {
        return new SectionDto { Id = section.Id, Title = section.Title, Description = section.Description };
    }

    public async Task<PaginatedResponse<CourseDto>> SearchAsync(CourseSearchRequest request)
    {
        try
        {
            IQueryable<Models.Entities.Course> query;

            if (request.IncludeDeleted)
            {
                query = _context.Courses
                    .IgnoreQueryFilters()
                    .Include(c => c.Author)
                    .Include(c => c.Tags)
                    .AsQueryable();
                if (request.Status.HasValue)
                    query = query.Where(c => c.Status == request.Status.Value);
            }
            else if (request.Status.HasValue)
            {
                query = _courseRepository.GetCoursesQuery()
                    .Where(c => c.Status == request.Status.Value)
                    .Include(c => c.Author);
            }
            else
            {
                query = _courseRepository.GetPublishedCoursesQuery();
            }

            var beforeFilter = await _courseRepository.GetTotalCountAsync(query);
            _logger.LogInformation("Курсов до фильтров: {Count}", beforeFilter);

            if (request.CategoryId.HasValue)
                query = query.Where(c => c.CategoryId == request.CategoryId.Value);

            if (request.Level.HasValue)
                query = query.Where(c => c.Level == request.Level.Value);

            if (request.IsFree.HasValue && request.IsFree.Value)
                query = query.Where(c => c.Price == 0);
            else
            {
                if (request.MinPrice.HasValue)
                    query = query.Where(c => c.Price >= Math.Round(request.MinPrice.Value, 2));
                if (request.MaxPrice.HasValue)
                    query = query.Where(c => c.Price <= Math.Round(request.MaxPrice.Value, 2));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = ApplySearchPriority(query, request.SearchTerm);

            var totalCount = await _courseRepository.GetTotalCountAsync(query);

            if (request.SortBy.HasValue)
                query = ApplySorting(query, request.SortBy.Value, request.SearchTerm);

            var items = await _courseRepository.GetCoursesWithProjectionAsync(
                query,
                (request.Page - 1) * request.PageSize,
                request.PageSize);

            _logger.LogInformation("Поиск курсов: SearchTerm={SearchTerm}, Найдено={TotalCount}", request.SearchTerm, totalCount);

            return new PaginatedResponse<CourseDto>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = request.Page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                PageSize = request.PageSize
            };
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске курсов: {SearchTerm}", request.SearchTerm);
            throw;
        }
    }

    private IQueryable<Models.Entities.Course> ApplySearchPriority(IQueryable<Models.Entities.Course> query, string searchTerm)
    {
        var s = searchTerm.ToLower();
        return query.Where(c => c.Title.ToLower() == s)
            .Union(query.Where(c => c.Title.ToLower().StartsWith(s) && c.Title.ToLower() != s))
            .Union(query.Where(c => c.Title.ToLower().Contains(s) && !c.Title.ToLower().StartsWith(s) && c.Title.ToLower() != s))
            .Union(query.Where(c => c.Description.ToLower().Contains(s) && !c.Title.ToLower().Contains(s)));
    }

    private IQueryable<Models.Entities.Course> ApplySorting(IQueryable<Models.Entities.Course> query, CoursesSortBy sortBy, string? searchTerm)
    {
        return sortBy switch
        {
            CoursesSortBy.Price => query.OrderBy(c => c.Price),
            CoursesSortBy.Title => query.OrderBy(c => c.Title),
            CoursesSortBy.Date => query.OrderBy(c => c.PublishedAt),
            CoursesSortBy.DateDesc => query.OrderByDescending(c => c.PublishedAt),
            CoursesSortBy.Relevance => ApplyRelevanceSorting(query, searchTerm),
            _ => query.OrderBy(c => c.Title)
        };
    }

    private IQueryable<Models.Entities.Course> ApplyRelevanceSorting(IQueryable<Models.Entities.Course> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return query.OrderBy(c => c.Title);
        var s = searchTerm.ToLower();
        return query
            .OrderBy(c => c.Title.ToLower() != s)
            .ThenBy(c => !c.Title.ToLower().StartsWith(s))
            .ThenBy(c => !c.Title.ToLower().Contains(s))
            .ThenBy(c => !c.Description.ToLower().Contains(s));
    }

    public async Task<ServiceResult<bool>> PublishCourseAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return ServiceResult<bool>.Fail($"Курс с id: {id} не найден", StatusCodes.Status404NotFound);
        if (course.Status == CourseStatus.Published)
            return ServiceResult<bool>.Fail($"Курс с id: {id} уже опубликован");
        if (string.IsNullOrWhiteSpace(course.Title) || string.IsNullOrWhiteSpace(course.Description))
            return ServiceResult<bool>.Fail("Курс должен иметь название и описание перед публикацией");

        course.Status = CourseStatus.Published;
        course.PublishedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<IList<CourseDto>>> GetMyCoursesAsync(Guid userId, CourseStatus? status = null)
    {
        var course = await _courseRepository.GetByAuthorIdAsync(userId, status);
        var result = course.Select(MapToDo).ToList();
        return ServiceResult<IList<CourseDto>>.Ok(result);
    }

    public async Task<ServiceResult<bool>> ModeratorCourseAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return ServiceResult<bool>.Fail($"Курс с id: {id} не найден", StatusCodes.Status404NotFound);
        if (course.Status != CourseStatus.Draft && course.Status != CourseStatus.RejectedByModerator)
            return ServiceResult<bool>.Fail("На модерацию можно отправить только курс в статусе Draft или RejectedByModerator");
        if (string.IsNullOrWhiteSpace(course.Title) || string.IsNullOrWhiteSpace(course.Description))
            return ServiceResult<bool>.Fail("Курс должен иметь название и описание перед отправкой на модерацию");

        course.Status = CourseStatus.UnderReview;
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> ApproveCourseAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return ServiceResult<bool>.Fail($"Курс с id: {id} не найден", StatusCodes.Status404NotFound);
        if (course.Status != CourseStatus.UnderReview)
            return ServiceResult<bool>.Fail("Одобрить можно только курс в статусе UnderReview");

        course.Status = CourseStatus.Published;
        course.PublishedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;

        if (course.ReviewerId.HasValue)
        {
            _context.CourseReviewHistories.Add(new CourseReviewHistory
            {
                CourseId = course.Id,
                ModeratorId = course.ReviewerId.Value,
                Decision = ReviewDecision.Approved,
                ReviewedAt = DateTime.UtcNow,
            });
        }
        course.ReviewerId = null;
        course.ReviewStartedAt = null;
        await _courseRepository.UpdateAsync(course);

        if (course.AuthorId.HasValue)
        {
            await _notificationService.SendAsync(
                course.AuthorId.Value,
                "Курс одобрен",
                $"Ваш курс «{course.Title}» прошёл модерацию и опубликован",
                NotificationType.CourseApproved,
                relatedEntityId: course.Id,
                relatedCourseId: course.Id);
        }
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> RejectCourseAsync(Guid id, string? reason)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return ServiceResult<bool>.Fail($"Курс с id: {id} не найден", StatusCodes.Status404NotFound);
        if (course.Status != CourseStatus.UnderReview)
            return ServiceResult<bool>.Fail("Отклонить можно только курс в статусе UnderReview");

        course.Status = CourseStatus.RejectedByModerator;
        course.UpdatedAt = DateTime.UtcNow;

        if (course.ReviewerId.HasValue)
        {
            _context.CourseReviewHistories.Add(new CourseReviewHistory
            {
                CourseId = course.Id,
                ModeratorId = course.ReviewerId.Value,
                Decision = ReviewDecision.Rejected,
                Reason = reason,
                ReviewedAt = DateTime.UtcNow,
            });
        }
        course.ReviewerId = null;
        course.ReviewStartedAt = null;
        await _courseRepository.UpdateAsync(course);

        if (course.AuthorId.HasValue)
        {
            await _notificationService.SendAsync(
                course.AuthorId.Value,
                "Курс отклонён",
                $"Ваш курс «{course.Title}» отклонён модератором.{(string.IsNullOrWhiteSpace(reason) ? "" : $" Причина: {reason}")}",
                NotificationType.CourseRejected,
                relatedEntityId: course.Id,
                relatedCourseId: course.Id);
        }
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<PaginatedResponse<CourseDto>> GetPendingCoursesAsync(int page, int pageSize)
    {
        var query = _courseRepository.GetPendingCoursesQuery();
        var totalCount = await _courseRepository.GetTotalCountAsync(query);
        var items = await _courseRepository.GetCoursesWithProjectionAsync(query, (page - 1) * pageSize, pageSize);

        return new PaginatedResponse<CourseDto>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            PageSize = pageSize
        };
    }
}
