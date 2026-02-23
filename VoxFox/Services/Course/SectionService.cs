
using VoxFox.Interfaces.Section;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Services
{
    public class SectionService : ISectionService
    {
        private readonly ISectionRepository _sectionRepository;

        public SectionService(ISectionRepository sectionRepository)
        {
            _sectionRepository = sectionRepository;
        }

        public async Task<SectionDto> CreateSectionAsync(Guid courseId, CreateSectionDto createSectionDto)
        {
            if (!await _sectionRepository.CourseExistsAsync(courseId))
            {
                throw new KeyNotFoundException($"Курс с id: {courseId} не найден");
            }

            var section = new Section
            {
                Title = createSectionDto.Title,
                Description = createSectionDto.Description,
                CourseId = courseId
            };

            var createSection = await _sectionRepository.AddAsync(section);
            if (createSection == null)
            {
                throw new Exception("Не удалось добавить раздел");
            }

            return MapToDo(createSection);
        }

        public async Task<ServiceResult<bool>> DeleteSectionAsync(Guid id)
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            if (section == null)
                return ServiceResult<bool>.Fail($"Раздел с id: {id} не найден", statusCode: StatusCodes.Status404NotFound);

            var isSuccess = await _sectionRepository.DeleteAsync(section);
            if (!isSuccess)
                return ServiceResult<bool>.Fail($"Не удалось удалить раздел по id: {id}", StatusCodes.Status500InternalServerError);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<bool> DeleteSectionsAsync(Guid id)
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            if (section == null)
                return false;

            var isSuccess = await _sectionRepository.DeleteAsync(section);
            return isSuccess;
        }

        public async Task<IReadOnlyCollection<SectionDto>> GetAllSectionsAsync()
        {
            var sections = await _sectionRepository.GetAllAsync();
            if (sections == null)
            {
                throw new Exception("Не удалось получить список разделов");
            }

            var sectionDTO = sections
                .Select(MapToDo)
                .ToList();

            return sectionDTO;
        }

        public async Task<SectionDto?> GetSectionByIdAsync(Guid id)
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            if (section == null)
                return null;

            var sectionDto = MapToDo(section);

            return sectionDto;
        }

        public async Task<IList<SectionDto>> GetSectionsByCourseIdAsync(Guid courseId)
        {
            var sections = await _sectionRepository.GetByCourseIdAsync(courseId);
            if (sections == null || !sections.Any())
            {
                return new List<SectionDto>();
            }

            return sections.Select(MapToDo).ToList();
        }

        public async Task<SectionDto> UpdateSectionAsync(Guid id, UpdateSectionDto updateSectionDto)
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            if (section == null)
            {
                throw new KeyNotFoundException($"Раздел с id: {id} не найден");
            }

            section.Title = updateSectionDto.Title ?? section.Title;
            section.Description = updateSectionDto.Description ?? section.Description;

            var updatedSection = await _sectionRepository.UpdateAsync(section);
            if (updatedSection == null)
            {
                throw new Exception("Не удалось обновить раздел");
            }

            return MapToDo(updatedSection);
        }

        private SectionDto MapToDo(Section section)
        {
            return new SectionDto
            {
                Id = section.Id,
                Title = section.Title,
                Description = section.Description
            };
        }
    }
}
