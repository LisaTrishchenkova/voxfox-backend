using VoxFox.Enums;
using VoxFox.Models.DTOs;
using VoxFox.Models.Requests;
using VoxFox.Models.Responses;

namespace VoxFox.Interfaces;

public interface ICourseService
{
    Task<IReadOnlyCollection<CourseDto>> GetAllCoursesAsync();
    Task<CourseDto?> GetCourseByIdAsync(Guid id);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto createCourseDto, Guid authorId);
    Task<ServiceResult<CourseDto>> UpdateCourseAsync(Guid id, UpdateCourseDto updateCourseDto, Guid userId);
    Task<ServiceResult<IList<SectionDto>>> GetSectionsByCourseIdAsync(Guid courseId);
    Task<bool> DeleteCourseAsync(Guid id, Guid userId, bool isAdmin);
    Task<PaginatedResponse<CourseDto>> SearchAsync(CourseSearchRequest request);
    Task<ServiceResult<bool>> PublishCourseAsync(Guid id);
    Task<ServiceResult<IList<CourseDto>>> GetMyCoursesAsync(Guid userId, CourseStatus? status = null);
    Task<ServiceResult<bool>> ModeratorCourseAsync(Guid id);
    Task<ServiceResult<bool>> ApproveCourseAsync(Guid id);
    Task<ServiceResult<bool>> RejectCourseAsync(Guid id, string? reason);
    Task<PaginatedResponse<CourseDto>> GetPendingCoursesAsync(int page, int pageSize);
}
