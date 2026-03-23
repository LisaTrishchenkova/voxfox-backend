using VoxFox.Interfaces.Section;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Services.Course
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

                return ServiceResult<SectionDto>.Created(MapToDo(createSection));
            }
            catch (Exception ex)
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

                var isSuccess = await _sectionRepository.DeleteSoftAsync(section);
                if (!isSuccess)
                    return ServiceResult<bool>.Fail($"Не удалось удалить раздел по id: {id}", StatusCodes.Status500InternalServerError);

                return ServiceResult<bool>.Ok(true, "Раздел успешно удален");
            }
            catch (Exception ex)
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

            var isSuccess = await _sectionRepository.DeleteSoftAsync(section);
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

                var sectionDto = sections
                    .Select(MapToDo)
                    .ToList();

                return ServiceResult<IReadOnlyCollection<SectionDto>>.Ok(sectionDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<IReadOnlyCollection<SectionDto>>.Fail(
                    $"Ошибка при получении списка разделов: {ex.Message}",
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<ServiceResult<IList<LessonDto>>> GetLessonssBySectionIdAsync(Guid sectionId)
        {
            try
            {
                var section = await _sectionRepository.GetByIdAsync(sectionId);
                if (section == null)
                {
                    return ServiceResult<IList<LessonDto>>.Fail(
                        $"Раздел с id: {sectionId} не найден",
                        StatusCodes.Status404NotFound
                    );
                }

                var lessons = await _sectionRepository.GetLessonBySectionIdAsync(sectionId);

                var lessonsDto = lessons.Select(MapToDo).ToList();
                return ServiceResult<IList<LessonDto>>.Ok(lessonsDto);
            }
            catch (Exception ex)
            {
                return ServiceResult<IList<LessonDto>>.Fail(
                    $"Ошибка при получении уроков раздела: {ex.Message}",
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
            catch (Exception ex)
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

                return ServiceResult<SectionDto>.Ok(MapToDo(updatedSection), "Раздел успешно обновлен");
            }
            catch (Exception ex)
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
        private LessonDto MapToDo(Lesson lesson)
        {
            return new LessonDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Content = lesson.Content!
            };
        }
    }
}
