using VoxFox.Models.Entities;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(Guid id);
    Task<Lesson> AddAsync(Lesson lesson);
    Task<Lesson> UpdateAsync(Lesson lesson);
    Task<bool> DeleteAsync(Lesson lesson);
    public Task<bool> SectionExistsAsync(Guid sectionId);
}
