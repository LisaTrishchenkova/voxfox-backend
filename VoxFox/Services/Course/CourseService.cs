using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;
using VoxFox;
using System.Diagnostics;

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
            Title = createCourseDto.Title,
            Description = createCourseDto.Description,
            Tags = createCourseDto.Tags.Select(tagDto => new Tag
            {
                Id = Guid.NewGuid(),
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
        course.Tags = updateCourseDto.Tags.Select(tagDto => new Tag
        {
            Name = tagDto.Name,
            CourseId = course.Id
        }).ToList();

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
        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Tags = course.Tags.Select(t => new TagDto
            {
                Name = t.Name
            }).ToList()
        };
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
           var stopwatch = Stopwatch.StartNew(); 
            try
            {
                // 1. Получаем базовый запрос из репозитория
                var query = _courseRepository.GetCoursesQuery();

                // 2. Применяем фильтры
                if (request.CategoryId.HasValue)
                {
                    // query = query.Where(c => c.CategoryId == request.CategoryId.Value);
                }

                // 3. Применяем поиск
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    query = ApplySearchPriority(query, request.SearchTerm);
                }

                // 4. Получаем общее количество через репозиторий
                var totalCount = await _courseRepository.GetTotalCountAsync(query);

                // 5. Применяем сортировку
                query = ApplySorting(query, request.SortBy, request.SearchTerm);

                // 6. Получаем данные через репозиторий с пагинацией
                var items = await _courseRepository.GetCoursesWithProjectionAsync(
                    query,
                    (request.Page - 1) * request.PageSize,
                    request.PageSize
                );

                // 7. Вычисляем страницы
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                // 8. Логируем
                _logger.LogInformation(
                    "Поиск курсов: SearchTerm={SearchTerm}, Найдено={TotalCount}, Время={ElapsedMs}ms",
                    request.SearchTerm, totalCount, stopwatch.ElapsedMilliseconds);

                // 9. Возвращаем результат
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

        // Приоритет 1: Точное совпадение названия
        var exactMatches = query.Where(c => c.Title.ToLower() == searchTermLower);
        
        // Приоритет 2: Название начинается с поискового запроса
        var startsWithMatches = query.Where(c => 
            c.Title.ToLower().StartsWith(searchTermLower) && 
            c.Title.ToLower() != searchTermLower);
        
        // Приоритет 3: Название содержит поисковый запрос
        var titleContainsMatches = query.Where(c => 
            c.Title.ToLower().Contains(searchTermLower) && 
            !c.Title.ToLower().StartsWith(searchTermLower) && 
            c.Title.ToLower() != searchTermLower);
        
        // Приоритет 4: Описание содержит поисковый запрос
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
}
