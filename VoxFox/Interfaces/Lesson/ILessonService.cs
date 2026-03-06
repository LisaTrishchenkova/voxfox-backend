using VoxFox.Models.DTOs;

namespace VoxFox.Interfaces.Section
{
    public interface ILessonService
    {
        Task<ServiceResult<LessonDto?>> GetLessonByIdAsync(Guid id);
        Task<ServiceResult<LessonDto>> CreateLessonAsync(Guid sectionId, CreateLessonDto createLessonDto);
        Task<ServiceResult<LessonDto>> UpdateLessonAsync(Guid id, UpdateLessonDto updateLessonDto);
        Task<ServiceResult<bool>> DeleteLessonAsync(Guid id);
    }
}
