using VoxFox.Models.DTOs;

namespace VoxFox.Interfaces.Lesson
{
    public interface ILessonService
    {
        Task<ServiceResult<LessonDto?>> GetLessonByIdAsync(Guid id);
        Task<ServiceResult<LessonDto>> CreateLessonAsync(Guid sectionId, CreateLessonDto createLessonDto);
        Task<ServiceResult<LessonDto>> UpdateLessonAsync(Guid id, UpdateLessonDto updateLessonDto);
        Task<ServiceResult<bool>> DeleteLessonAsync(Guid id);
        Task<ServiceResult<LessonProgressDto>> CompleteLessonAsync(Guid lessonId, Guid userId);
        Task<ServiceResult<bool>> ReorderLessonsAsync(Guid sectionId, List<Guid> lessonIds);
    }
}
