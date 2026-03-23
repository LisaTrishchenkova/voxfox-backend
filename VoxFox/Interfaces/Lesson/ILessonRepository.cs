namespace VoxFox.Interfaces.Lesson;

public interface ILessonRepository
{
    Task<Models.Entities.Lesson?> GetByIdAsync(Guid id);
    Task<Models.Entities.Lesson> AddAsync(Models.Entities.Lesson lesson);
    Task<Models.Entities.Lesson> UpdateAsync(Models.Entities.Lesson lesson);
    Task<bool> DeleteSoftAsync(Models.Entities.Lesson lesson);
    public Task<bool> SectionExistsAsync(Guid sectionId);
}