using System.Collections;
using VoxFox.Models.Entities;

public interface ICourseRepository
{
    Task<IList<Course>> GetAllAsync();
    Task<Course> GetByIdAsync(Guid id);
    Task<Course> AddAsync(Course course);
    Task<Course> UpdateAsync(Course course);
    Task<bool> DeleteAsync(Guid id);
}
