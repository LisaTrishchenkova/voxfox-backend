namespace VoxFox.Interfaces.User;

public interface IFileStorageService
{
	Task<ServiceResult<string>> SaveAvatarAsync(Guid userId, IFormFile file);
	Task<ServiceResult<bool>> DeleteAvatarAsync(string url);

	Task<ServiceResult<string>> SaveCourseCoverAsync(Guid courseId, IFormFile file);
	Task<ServiceResult<bool>> DeleteCourseCoverAsync(string url);
}
