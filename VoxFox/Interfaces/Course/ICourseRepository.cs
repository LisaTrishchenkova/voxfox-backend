using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Interfaces.Course;

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

    IQueryable<Models.Entities.Course> GetCoursesQuery();
    IQueryable<Models.Entities.Course> GetPublishedCoursesQuery();
    Task<List<CourseDto>> GetCoursesWithProjectionAsync(
        IQueryable<Models.Entities.Course> query,
        int skip,
        int take);
    Task<int> GetTotalCountAsync(IQueryable<Models.Entities.Course> query);
    System.Threading.Tasks.Task DeleteTagAsync(Tag existingTag);
}
