using VoxFox.Models.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public interface ICourseService
{
    Task<IReadOnlyCollection<CourseDto>> GetAllCoursesAsync();
    Task<CourseDto> GetCourseByIdAsync(Guid id);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto);
    Task<CourseDto> UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto);
    Task<bool> DeleteCourseAsync(Guid id);
}
