using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;
using VoxFox;
using VoxFox.Enums;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<CourseService> _logger;

    public CourseService(ICourseRepository courseRepository, ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto)
    {
        var course = new Course
        {
            Status = CourseStatus.Draft,
            Title = createCourseDto.Title,
            Description = createCourseDto.Description,
            CategoryId = createCourseDto.CategoryId,
            AuthorId = createCourseDto.AuthorId,
            PublishedAt = DateTime.UtcNow,
            Tags = createCourseDto.Tags.Select(tagDto => new Tag
            {
                Name = tagDto.Name
            }).ToList()
        };

        var createdCourse = await _courseRepository.AddAsync(course);
        if (createdCourse == null)
        {
            throw new Exception("Не удалось добавить курс");
        }
        return MapToDo(createdCourse);
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return false;

        var isSuccess = await _courseRepository.DeleteSoftAsync(course);
        return isSuccess;
    }

    public async Task<IReadOnlyCollection<CourseDto>> GetAllCoursesAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        if (courses == null)
            throw new Exception("Не удалось получить список курсов");

        var coursesDTO = courses
            .Select(MapToDo)
            .ToList();

        return coursesDTO;
    }

    public async Task<CourseDto?> GetCourseByIdAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return null;

        var courseDTO = MapToDo(course);

        return courseDTO;
    }

    public async Task<CourseDto> UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            throw new Exception($"Не удалось получить курс по id: {id}");

        course.Title = updateCourseDto.Title ?? course.Title;
        course.Description = updateCourseDto.Description ?? course.Description;

        // Обновляем теги
        if (updateCourseDto.Tags != null && course.Tags != null)
        {
            // Удаляем старые теги, которых нет в новом списке
            var existingTags = course.Tags.ToList();
            var newTagNames = updateCourseDto.Tags.Select(t => t.Name).ToList();

            // Удаляем теги, которых нет в новом списке
            foreach (var existingTag in existingTags)
            {
                if (!newTagNames.Contains(existingTag.Name))
                {
                    // _context.Tags.Remove(existingTag); // или course.Tags.Remove(existingTag)
                    await _courseRepository.DeleteTagAsync(existingTag);
                }
            }

            // Обновляем существующие и добавляем новые теги
            foreach (var tagDto in updateCourseDto.Tags)
            {
                var existingTag = course.Tags.FirstOrDefault(t => t.Name == tagDto.Name);
                if (existingTag == null)
                {
                    // Добавляем новый тег
                    course.Tags.Add(new Tag
                    {
                        Name = tagDto.Name,
                        CourseId = course.Id
                    });
                }
                // Если тег существует, ничего не делаем (оставляем как есть)
            }
        }

        var updateCourse = await _courseRepository.UpdateAsync(course);
        return MapToDo(updateCourse);
    }

    public async Task<ServiceResult<IList<SectionDto>>> GetSectionsByCourseIdAsync(Guid courseId)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                return ServiceResult<IList<SectionDto>>.Fail(
                    $"Курс с id: {courseId} не найден",
                    StatusCodes.Status404NotFound
                );
            }

            var sections = await _courseRepository.GetSectionsByCourseIdAsync(courseId);

            var sectionsDto = sections.Select(MapToDo).ToList();
            return ServiceResult<IList<SectionDto>>.Ok(sectionsDto);
        }
        catch (System.Exception ex)
        {
            return ServiceResult<IList<SectionDto>>.Fail(
                $"Ошибка при получении разделов курса: {ex.Message}",
                StatusCodes.Status500InternalServerError
            );
        }
    }

    private CourseDto MapToDo(Course course)
    {
        var courses = new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Status = course.Status,
            CategoryId = course.CategoryId,
            Tags = course.Tags?.Select(t => new TagDto
            {
                Name = t.Name
            }).ToList(),
             Author = new AuthorDto 
            {
            Id = course.Author.Id,
            Name = course.Author.Name
            },
             PublishedAt = course.PublishedAt
        };

        return courses;
    }
    private SectionDto MapToDo(Section section)
    {
        return new SectionDto
        {
            Id = section.Id,
            Title = section.Title,
            Description = section.Description
        };
    }

    public async Task<PaginatedResponse<CourseDto>> SearchAsync(CourseSearchRequest request)
    {
        try
        {
            var query = _courseRepository.GetPublishedCoursesQuery();

            if (request.CategoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == request.CategoryId.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = ApplySearchPriority(query, request.SearchTerm);
            }

            var totalCount = await _courseRepository.GetTotalCountAsync(query);

            if (request.SortBy.HasValue)
            {
                query = ApplySorting(query, request.SortBy.Value, request.SearchTerm);
            }

            var skip = (request.Page - 1) * request.PageSize;
            var take = request.PageSize;

            var items = await _courseRepository.GetCoursesWithProjectionAsync(
                query,
                skip,
                take
            );
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            _logger.LogInformation(
                "Поиск курсов: SearchTerm={SearchTerm}, Найдено={TotalCount}",
                request.SearchTerm, totalCount);
            return new PaginatedResponse<CourseDto>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = request.Page,
                TotalPages = totalPages,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске курсов: {SearchTerm}", request.SearchTerm);
            throw;
        }
    }

    private IQueryable<Course> ApplySearchPriority(IQueryable<Course> query, string searchTerm)
    {
        var searchTermLower = searchTerm.ToLower();

        var exactMatches = query.Where(c => c.Title.ToLower() == searchTermLower);

        var startsWithMatches = query.Where(c =>
            c.Title.ToLower().StartsWith(searchTermLower) &&
            c.Title.ToLower() != searchTermLower);

        var titleContainsMatches = query.Where(c =>
            c.Title.ToLower().Contains(searchTermLower) &&
            !c.Title.ToLower().StartsWith(searchTermLower) &&
            c.Title.ToLower() != searchTermLower);

        var descriptionContainsMatches = query.Where(c =>
            c.Description.ToLower().Contains(searchTermLower) &&
            !c.Title.ToLower().Contains(searchTermLower));

        return exactMatches
            .Union(startsWithMatches)
            .Union(titleContainsMatches)
            .Union(descriptionContainsMatches);
    }
    private IQueryable<Course> ApplySorting(IQueryable<Course> query, CoursesSortBy sortBy, string? searchTerm)
    {
        return sortBy switch
        {
            // CoursesSortBy.Price => query.OrderBy(c => c.Price),
            CoursesSortBy.Title => query.OrderBy(c => c.Title),
            CoursesSortBy.Date => query.OrderBy(c => c.PublishedAt),
            CoursesSortBy.DateDesc => query.OrderByDescending(c => c.PublishedAt),
            CoursesSortBy.Relevance => ApplyRelevanceSorting(query, searchTerm),
            _ => query.OrderBy(c => c.Title)
        };
    }

    private IQueryable<Course> ApplyRelevanceSorting(IQueryable<Course> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query.OrderBy(c => c.Title);

        var searchTermLower = searchTerm.ToLower();

        return query
            .OrderBy(c => c.Title.ToLower() != searchTermLower)          // Точные совпадения
            .ThenBy(c => !c.Title.ToLower().StartsWith(searchTermLower)) // Начинаются с
            .ThenBy(c => !c.Title.ToLower().Contains(searchTermLower))   // Содержат в названии
            .ThenBy(c => !c.Description.ToLower().Contains(searchTermLower)); // Содержат в описании
    }

    public async Task<ServiceResult<bool>> PublishCourseAsync(Guid id)
    {

        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
        {
            return ServiceResult<bool>.Fail(
                $"Курс с id: {id} не найден",
                StatusCodes.Status404NotFound
            );
        }

        if (course.Status == CourseStatus.Published)
        {
            return ServiceResult<bool>.Fail(
                $"Курс с id: {id} уже опубликован",
                StatusCodes.Status400BadRequest
            );
        }

        if (string.IsNullOrWhiteSpace(course.Title) ||
            string.IsNullOrWhiteSpace(course.Description))
        {
            return ServiceResult<bool>.Fail(
                "Курс должен иметь название и описание перед публикацией",
                StatusCodes.Status400BadRequest
            );
        }

        course.Status = CourseStatus.Published;
        course.PublishedAt = DateTime.UtcNow;

        await _courseRepository.UpdateAsync(course);

        return ServiceResult<bool>.Ok(true);
    }
}
