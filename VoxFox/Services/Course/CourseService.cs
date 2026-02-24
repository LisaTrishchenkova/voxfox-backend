using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;
using VoxFox;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;

    public CourseService(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
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

        var isSuccess = await _courseRepository.DeleteAsync(course);
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
                throw new Exception($"Не удалось получить курс по id: {courseId}");

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

}
