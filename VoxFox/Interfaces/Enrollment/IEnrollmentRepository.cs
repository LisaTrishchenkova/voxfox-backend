namespace VoxFox.Interfaces.Enrollment;

public interface IEnrollmentRepository
{
	Task<Models.Entities.Enrollment> AddAsync(Models.Entities.Enrollment enrollment);
	Task<Models.Entities.Enrollment?> GetByIdAsync(Guid id);
	Task<Models.Entities.Enrollment?> GetByUserAndCourseAsync(Guid userId, Guid courseId);
	Task<IList<Models.Entities.Enrollment>> GetByUserIdAsync(Guid userId);
	Task<bool> ExistsAsync(Guid userId, Guid courseId);
	Task<Models.Entities.Enrollment> UpdateAsync(Models.Entities.Enrollment enrollment);
	Task<bool> DeleteAsync(Models.Entities.Enrollment enrollment);
	Task<IList<Models.Entities.Enrollment>> GetByCourseIdAsync(Guid courseId);
}
