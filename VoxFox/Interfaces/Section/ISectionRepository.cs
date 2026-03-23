namespace VoxFox.Interfaces.Section;

public interface ISectionRepository
{
    Task<IList<Models.Entities.Section>?> GetAllAsync();
    Task<Models.Entities.Section?> GetByIdAsync(Guid id);
    Task<Models.Entities.Section> AddAsync(Models.Entities.Section section);
    Task<Models.Entities.Section> UpdateAsync(Models.Entities.Section section);
    Task<bool> DeleteSoftAsync(Models.Entities.Section section);
    public Task<bool> CourseExistsAsync(Guid courseId);
    Task<IList<Models.Entities.Lesson>> GetLessonBySectionIdAsync(Guid sectionId);

}