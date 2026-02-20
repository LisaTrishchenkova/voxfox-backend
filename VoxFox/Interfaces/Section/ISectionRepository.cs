using System.Collections;
using VoxFox.Models.Entities;

public interface ISectionRepository
{
    Task<IList<Section>?> GetAllAsync();
    Task<Section?> GetByIdAsync(Guid id);
    Task<Section> AddAsync(Section section);
    Task<Section> UpdateAsync(Section section);
    Task<bool> DeleteAsync(Section section);
    public Task<bool> CourseExistsAsync(Guid courseId);
    Task<IList<Section>> GetByCourseIdAsync(Guid courseId);
}
