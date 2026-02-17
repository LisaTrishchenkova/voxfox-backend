using VoxFox.Models.Entities;

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
            Tags = createCourseDto.Tags
        };

        var createdCourse = await _courseRepository.AddAsync(course);

        return MapToDo(createdCourse);
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
        {
            return false;
        }

        var isSuccess = await _courseRepository.DeleteAsync(course);
        return isSuccess;
    }

    public async Task<IReadOnlyCollection<CourseDto>> GetAllCoursesAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        return courses.Select(course => MapToDo(course)).ToList();
    }

    public async Task<CourseDto> GetCourseByIdAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return null;
        return MapToDo(course);
    }

    public async Task<CourseDto> UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto)
    {
        var existingCourse = await _courseRepository.GetByIdAsync(id);
        if (existingCourse == null)
            return null;
        existingCourse.Title = updateCourseDto.Title;
        existingCourse.Description = updateCourseDto.Description;
        existingCourse.Tags = updateCourseDto.Tags;
        var updateCourse = await _courseRepository.UpdateAsync(existingCourse);
        return MapToDo(updateCourse);
    }

    private CourseDto MapToDo(Course course)
    {
        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Tags = course.Tags
        };
    }
}
