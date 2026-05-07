using VoxFox.Models.DTOs.Admin;

namespace VoxFox.Interfaces.Admin;

public interface IAdminService
{
	Task<AdminStatsDto> GetStatsAsync();
	Task<IList<ModeratorStatsDto>> GetModeratorsStatsAsync();
	Task<ServiceResult<bool>> BlockUserAsync(Guid userId, string? reason);
	Task<ServiceResult<bool>> UnblockUserAsync(Guid userId);
	Task<ServiceResult<bool>> UnpublishCourseAsync(Guid courseId);
	Task<ServiceResult<bool>> ForceReleaseCourseAsync(Guid courseId);

}
