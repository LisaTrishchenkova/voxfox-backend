using VoxFox.Models.DTOs;

namespace VoxFox.Interfaces.Teacher;

public interface ITeacherService
{
	Task<ServiceResult<TeacherStatsDto>> GetStatsAsync(Guid teacherId);
	Task<ServiceResult<List<TeacherCourseStatsDto>>> GetCourseStatsAsync(Guid teacherId);
}
