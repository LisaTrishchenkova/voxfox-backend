using VoxFox.Models.DTOs;
using VoxFox.Models.Requests;
using VoxFox.Models.Responses;

namespace VoxFox.Interfaces.Course;

public interface ICourseService
{
    Task<IReadOnlyCollection<CourseDto>> GetAllCoursesAsync();
    Task<CourseDto?> GetCourseByIdAsync(Guid id);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto);
    Task<ServiceResult<CourseDto>> UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto);
    Task<ServiceResult<IList<SectionDto>>> GetSectionsByCourseIdAsync(Guid courseId);
    Task<bool> DeleteCourseAsync(Guid id);
    Task<PaginatedResponse<CourseDto>> SearchAsync(CourseSearchRequest request);
    Task<ServiceResult<bool>> PublishCourseAsync(Guid id);
    Task<ServiceResult<IList<CourseDto>>> GetMyCoursesAsync(Guid authorId);
    Task<ServiceResult<bool>> ModeratorCourseAsync(Guid id);
    Task<ServiceResult<bool>> ApproveCourseAsync(Guid id);
    Task<ServiceResult<bool>> RejectCourseAsync(Guid id, string? reason);
}
