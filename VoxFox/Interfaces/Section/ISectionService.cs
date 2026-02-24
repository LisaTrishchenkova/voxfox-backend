using System.Collections;
using VoxFox.Models.DTOs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace VoxFox.Interfaces.Section
{
    public interface ISectionService
    {
        Task<ServiceResult<IReadOnlyCollection<SectionDto>>> GetAllSectionsAsync();
        Task<ServiceResult<SectionDto?>> GetSectionByIdAsync(Guid id);
        Task<ServiceResult<SectionDto>> CreateSectionAsync(Guid courseId, CreateSectionDto createSectionDto);
        Task<ServiceResult<SectionDto>> UpdateSectionAsync(Guid id, UpdateSectionDto updateSectionDto);
        Task<ServiceResult<bool>> DeleteSectionAsync(Guid id);
    }
}
