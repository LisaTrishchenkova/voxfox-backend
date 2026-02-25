
using System.Collections;
using VoxFox.Interfaces.Section;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Services
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;

        public LessonService(ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
        }

        public async Task<ServiceResult<LessonDto>> CreateLessonAsync(Guid sectionId, CreateLessonDto createLessonDto)
        {
             try
            {
                var section = await _lessonRepository.SectionExistsAsync(sectionId);
                if (!section)
                {
                    return ServiceResult<LessonDto>.Fail(
                        $"Раздел с id: {sectionId} не найден",
                        StatusCodes.Status404NotFound
                    );
                }

                var lesson = new Lesson
                {
                    Title = createLessonDto.Title,
                    Description = createLessonDto.Description,
                    Content = createLessonDto.Content,
                    SectionId = sectionId
                };

                var createLesson = await _lessonRepository.AddAsync(lesson);
                if (createLesson == null)
                {
                    return ServiceResult<LessonDto>.Fail(
                        $"Не удалось создать урок",
                        StatusCodes.Status404NotFound
                    );
                }

                return ServiceResult<LessonDto>.Created(MapToDo(createLesson));
            }
            catch (System.Exception ex)
            {
                return ServiceResult<LessonDto>.Fail(
                    $"Ошибка при создании урока: {ex.Message}",
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<ServiceResult<bool>> DeleteLessonAsync(Guid id)
        {
            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);
                if (lesson == null)
                    return ServiceResult<bool>.Fail($"Урок с id: {id} не найден", statusCode: StatusCodes.Status404NotFound);

                var isSuccess = await _lessonRepository.DeleteAsync(lesson);
                if (!isSuccess)
                    return ServiceResult<bool>.Fail($"Не удалось удалить урок по id: {id}", StatusCodes.Status500InternalServerError);

                return ServiceResult<bool>.Ok(true, "Урок успешно удален");
            }
            catch (System.Exception ex)
            {
                return ServiceResult<bool>.Fail(
                     $"Ошибка при удалении урока: {ex.Message}",
                     StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<ServiceResult<LessonDto?>> GetLessonByIdAsync(Guid id)
        {
            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);
                if (lesson == null)
                {
                    return ServiceResult<LessonDto?>.Fail(
                        $"Урок с id: {id} не найден",
                        StatusCodes.Status404NotFound
                    );
                }

                return ServiceResult<LessonDto?>.Ok(MapToDo(lesson));
            }
            catch (System.Exception ex)
            {
                return ServiceResult<LessonDto?>.Fail(
                    $"Ошибка при получении урока: {ex.Message}",
                    StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<ServiceResult<LessonDto>> UpdateLessonAsync(Guid id, UpdateLessonDto updateLessonDto)
        {
             try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);
                if (lesson == null)
                {
                    return ServiceResult<LessonDto>.Fail(
                        $"Урок с id: {id} не найден",
                        StatusCodes.Status404NotFound
                    );
                }

                lesson.Title = updateLessonDto.Title ?? lesson.Title;
                lesson.Description = updateLessonDto.Description ?? lesson.Description;
                lesson.Content = updateLessonDto.Content ?? lesson.Content;

                var updatedLesson = await _lessonRepository.UpdateAsync(lesson);
                if (updatedLesson == null)
                {
                    return ServiceResult<LessonDto>.Fail(
                        "Не удалось обновить урок",
                        StatusCodes.Status500InternalServerError
                    );
                }

                return ServiceResult<LessonDto>.Ok(MapToDo(updatedLesson), "Урок успешно обновлен");
            }
            catch (System.Exception ex)
            {
                return ServiceResult<LessonDto>.Fail(
                    $"Ошибка при обновлении урока: {ex.Message}",
                    StatusCodes.Status500InternalServerError
                );
            }
        }

          private LessonDto MapToDo(Lesson lesson)
        {
            return new LessonDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Content = lesson.Content
            };
        }
    }
}
