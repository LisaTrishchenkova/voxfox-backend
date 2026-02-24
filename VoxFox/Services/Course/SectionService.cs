
using System.Collections;
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

        public async Task<ServiceResult<SectionDto>> CreateSectionAsync(Guid courseId, CreateSectionDto createSectionDto)
        {
            try
            {
                var newSection = await _sectionRepository.CourseExistsAsync(courseId);
                if (!newSection)
                {
                    return ServiceResult<SectionDto>.Fail(
                        $"Курс с id: {courseId} не найден",
                        StatusCodes.Status404NotFound
                    );
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
                    return ServiceResult<SectionDto>.Fail(
                        $"Не удалось создать раздел",
                        StatusCodes.Status404NotFound
                    );
                }

                return ServiceResult<SectionDto>.Created(MapToDo(createSection));
            }
            catch (System.Exception ex)
            {
                return ServiceResult<SectionDto>.Fail(
                    $"Ошибка при создании раздела: {ex.Message}",
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<ServiceResult<bool>> DeleteSectionAsync(Guid id)
        {
            try
            {
                var section = await _sectionRepository.GetByIdAsync(id);
                if (section == null)
                    return ServiceResult<bool>.Fail($"Раздел с id: {id} не найден", statusCode: StatusCodes.Status404NotFound);

                var isSuccess = await _sectionRepository.DeleteAsync(section);
                if (!isSuccess)
                    return ServiceResult<bool>.Fail($"Не удалось удалить раздел по id: {id}", StatusCodes.Status500InternalServerError);

                return ServiceResult<bool>.Ok(true, "Раздел успешно удален");
            }
            catch (System.Exception ex)
            {
                return ServiceResult<bool>.Fail(
                     $"Ошибка при удалении раздела: {ex.Message}",
                     StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<bool> DeleteSectionsAsync(Guid id)
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            if (section == null)
                return false;

            var isSuccess = await _sectionRepository.DeleteAsync(section);
            return isSuccess;
        }

        public async Task<ServiceResult<IReadOnlyCollection<SectionDto>>> GetAllSectionsAsync()
        {
            try
            {
                var sections = await _sectionRepository.GetAllAsync();
                if (sections == null)
                {
                    return ServiceResult<IReadOnlyCollection<SectionDto>>.Fail(
                        "Не удалось получить список разделов",
                        StatusCodes.Status500InternalServerError
                    );
                }

                var sectionDTO = sections
                    .Select(MapToDo)
                    .ToList();

                return ServiceResult<IReadOnlyCollection<SectionDto>>.Ok(sectionDTO);
            }
            catch (System.Exception ex)
            {
                return ServiceResult<IReadOnlyCollection<SectionDto>>.Fail(
                    $"Ошибка при получении списка разделов: {ex.Message}",
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<ServiceResult<SectionDto?>> GetSectionByIdAsync(Guid id)
        {
            try
            {
                var section = await _sectionRepository.GetByIdAsync(id);
                if (section == null)
                {
                    return ServiceResult<SectionDto?>.Fail(
                        $"Раздел с id: {id} не найден",
                        StatusCodes.Status404NotFound
                    );
                }

                return ServiceResult<SectionDto?>.Ok(MapToDo(section));
            }
            catch (System.Exception ex)
            {
                return ServiceResult<SectionDto?>.Fail(
                    $"Ошибка при получении раздела: {ex.Message}",
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<ServiceResult<SectionDto>> UpdateSectionAsync(Guid id, UpdateSectionDto updateSectionDto)
        {
            try
            {
                var section = await _sectionRepository.GetByIdAsync(id);
                if (section == null)
                {
                    return ServiceResult<SectionDto>.Fail(
                        $"Раздел с id: {id} не найден",
                        StatusCodes.Status404NotFound
                    );
                }

                section.Title = updateSectionDto.Title ?? section.Title;
                section.Description = updateSectionDto.Description ?? section.Description;

                var updatedSection = await _sectionRepository.UpdateAsync(section);
                if (updatedSection == null)
                {
                    return ServiceResult<SectionDto>.Fail(
                        "Не удалось обновить раздел",
                        StatusCodes.Status500InternalServerError
                    );
                }

                return ServiceResult<SectionDto>.Ok(MapToDo(updatedSection), "Раздел успешно обновлен");
            }
            catch (System.Exception ex)
            {
                return ServiceResult<SectionDto>.Fail(
                    $"Ошибка при обновлении раздела: {ex.Message}",
                    StatusCodes.Status500InternalServerError
                );
            }
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
