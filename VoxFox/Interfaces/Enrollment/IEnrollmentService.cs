using VoxFox.Enums;
using VoxFox.Models.DTOs;

namespace VoxFox.Interfaces.Enrollment;

public interface IEnrollmentService
{
	Task<ServiceResult<EnrollmentDto>> EnrollAsync(Guid courseId, Guid userId);
	Task<ServiceResult<bool>> CancelEnrollmentAsync(Guid enrollmentId, Guid userId);
	Task<ServiceResult<IList<EnrollmentDto>>> GetUserEnrollmentsAsync(Guid userId);
	Task<ServiceResult<IList<EnrollmentDto>>> GetCourseEnrollmentsAsync(Guid courseId, Guid requesterId,
		UserRole? requesterRole);
}
