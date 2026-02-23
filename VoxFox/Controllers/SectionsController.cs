using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using VoxFox.Interfaces.Section;
using VoxFox.Models.DTOs;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SectionsController : ControllerBase
    {
        //Зарегистрировать не забыть в Program.cs
        private readonly ISectionService _sectionService;
        public SectionsController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpPost]
        public async Task<ActionResult<SectionDto>> CreateSection(
            Guid courseId,
            CreateSectionDto createSection
        )
        {
            try
            {
                var course = await _sectionService.CreateSectionAsync(courseId, createSection);
                return CreatedAtAction(nameof(GetSectionById), new { id = course.Id }, course);
            }
            catch (System.Exception ex)
            {
                var message = ex.Message;
                return StatusCode(500, message);
            }
        }

        // [HttpGet]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SectionDto))]
        // public async Task<ActionResult<IList<SectionDto>>> GetAllSections()
        // {
        //     try
        //     {
        //         var sections = await _sectionService.GetAllSectionsAsync();
        //         return Ok(sections);
        //     }
        //     catch (Exception)
        //     {
        //         return StatusCode(500, "Ошибка сервера");
        //     }
        // }

        [HttpGet("course/{courseId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SectionDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetSectionsByCourseId(
            [FromRoute, Required] Guid courseId
        )
        {
            try
            {
                var sections = await _sectionService.GetSectionsByCourseIdAsync(courseId);
                return Ok(sections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SectionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseDto>> GetSectionById(
            [FromRoute, Required] Guid id
        )
        {
            try
            {
                var section = await _sectionService.GetSectionByIdAsync(id);
                if (section == null)
                    return NotFound($"Не найден раздел по id: {id}");

                return Ok(section);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSectionById(
            [FromRoute] Guid id
        )
        {
            var result = await _sectionService.DeleteSectionAsync(id);

            if (!result.Success)
                return StatusCode(result.StatusCode ?? 400, result.Message);

            // TODO: Удалить позже после того как не нужно будет
            // var resultDeleted = await _sectionService.DeleteSectionsAsync(id);

            // if (!resultDeleted)
            //     return NotFound($"Не удалось удалить раздел по id: {id}");

            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseDto>> UpdateSection(
            [FromRoute] Guid id,
            [FromBody] UpdateSectionDto updateSectionDto
        )
        {
            var sectionUpdated = await _sectionService.UpdateSectionAsync(id, updateSectionDto);
            if (sectionUpdated == null)
                return NotFound($"Не удалось обновить раздел по id: {id}");

            return Ok(sectionUpdated);
        }
    }
}
