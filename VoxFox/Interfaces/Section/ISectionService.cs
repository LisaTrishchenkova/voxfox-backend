using System.Collections;
using VoxFox.Models.DTOs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace VoxFox.Interfaces.Section
{
    public interface ISectionService
    {
        Task<IReadOnlyCollection<SectionDto>> GetAllSectionsAsync();
        Task<SectionDto?> GetSectionByIdAsync(Guid id);
        Task<SectionDto> CreateSectionAsync(Guid courseId, CreateSectionDto createSectionDto);
        Task<SectionDto> UpdateSectionAsync(Guid id, UpdateSectionDto updateSectionDto);
        Task<bool> DeleteSectionsAsync(Guid id);
        Task<IList<SectionDto>> GetSectionsByCourseIdAsync(Guid courseId);
    }
}
