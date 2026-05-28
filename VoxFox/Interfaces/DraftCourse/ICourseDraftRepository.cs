using VoxFox.Models.Entities;

namespace VoxFox.Interfaces.DraftCourse;

public interface ICourseDraftRepository
{
    Task<CourseDraft?> GetByIdAsync(Guid id);
    Task<CourseDraft?> GetByCourseIdAsync(Guid courseId);
    Task<List<CourseDraft>> GetPendingAsync();
    Task<CourseDraft> AddAsync(CourseDraft draft);
    System.Threading.Tasks.Task UpdateAsync(CourseDraft draft);
    System.Threading.Tasks.Task DeleteAsync(CourseDraft draft);
}
