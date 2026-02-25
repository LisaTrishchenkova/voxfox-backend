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
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SectionDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SectionDto>> CreateSection(
            Guid courseId,
            CreateSectionDto createSection
        )
        {
            try
            {
                var result = await _sectionService.CreateSectionAsync(courseId, createSection);

                if (!result.Success)
                {
                    return StatusCode(result.StatusCode ?? 400, result.Message);
                }

                return CreatedAtAction(
                    nameof(GetSectionById),
                    new { id = result.Data.Id },
                    result.Data);
            }
            catch (System.Exception ex)
            {
                var message = ex.Message;
                return StatusCode(500, message);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SectionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CourseDto>> GetSectionById(
            [FromRoute, Required] Guid id
        )
        {
            try
            {
                var result = await _sectionService.GetSectionByIdAsync(id);
                if (!result.Success)
                {
                    return StatusCode(result.StatusCode ?? 404, result.Message);
                }

                return Ok(result.Data);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSectionById(
            [FromRoute] Guid id
        )
        {
            try
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
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CourseDto>> UpdateSection(
            [FromRoute] Guid id,
            [FromBody] UpdateSectionDto updateSectionDto
        )
        {
            try
            {
                var result = await _sectionService.UpdateSectionAsync(id, updateSectionDto);

                if (!result.Success)
                {
                    return StatusCode(result.StatusCode ?? 400, result.Message);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
