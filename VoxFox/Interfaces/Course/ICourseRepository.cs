using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Interfaces;

public interface ICourseRepository
{
    Task<IList<Models.Entities.Course>?> GetAllAsync();
    Task<Models.Entities.Course?> GetByIdAsync(Guid id);
    Task<Models.Entities.Course> AddAsync(Models.Entities.Course course);
    Task<Models.Entities.Course> UpdateAsync(Models.Entities.Course course);
    Task<bool> DeleteSoftAsync(Models.Entities.Course course);
    Task<bool> ExistCourseByIdAsync(Guid id);
    Task<IList<Models.Entities.Section>> GetSectionsByCourseIdAsync(Guid courseId);
    Task<IList<Models.Entities.Course>> GetByAuthorIdAsync(Guid userId);

	IQueryable<Course> GetCoursesQuery();
	IQueryable<Course> GetPublishedCoursesQuery();

	Task<List<CourseDto>> GetCoursesWithProjectionAsync(
		IQueryable<Course> query,
		int skip,
		int take);

	Task<int> GetTotalCountAsync(IQueryable<Course> query);
	System.Threading.Tasks.Task DeleteTagAsync(Tag existingTag);
	Task<List<Course>> GetForReindexAsync(int skip, int take, CancellationToken ct);
}
