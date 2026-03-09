using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using VoxFox.Interfaces.Section;
using VoxFox.Models.DTOs;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LessonController : ControllerBase
    {
        //TODO: Зарегистрировать не забыть в Program.cs
        private readonly ILessonService _lessonService;

        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(LessonDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LessonDto>> CreateLesson(
            Guid sectionId,
            CreateLessonDto createLesson
        )
        {
            try
            {
                var result = await _lessonService.CreateLessonAsync(sectionId, createLesson);

                if (!result.Success)
                {
                    return StatusCode(result.StatusCode ?? 400, result.Message);
                }

                return CreatedAtAction(
                    nameof(GetLessonById),
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LessonDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LessonDto>> GetLessonById(
            [FromRoute, Required] Guid id
        )
        {
            try
            {
                var result = await _lessonService.GetLessonByIdAsync(id);
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
        public async Task<IActionResult> DeleteLessonById(
            [FromRoute] Guid id
        )
        {
            try
            {
                var result = await _lessonService.DeleteLessonAsync(id);
                if (!result.Success)
                    return StatusCode(result.StatusCode ?? 400, result.Message);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LessonDto>> UpdateLesson(
            [FromRoute] Guid id,
            [FromBody] UpdateLessonDto updateLesson
        )
        {
            try
            {
                var result = await _lessonService.UpdateLessonAsync(id, updateLesson);

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
