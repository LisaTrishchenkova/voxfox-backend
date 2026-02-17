using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpPost]
        public async Task<ActionResult<CourseDto>> CreateCourse(
            CreateCourseDto createCourseDto
        )
        {
            try
            {
                var course = await _courseService.CreateCourseAsync(createCourseDto);
                return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course);
            }
            catch (System.Exception ex)
            {
                var message = ex.Message;
                return StatusCode(500, message);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseDto))]
        public async Task<ActionResult<IList<CourseDto>>> GetAllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return Ok(courses);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ошибка сервера");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseDto>> GetCourseById(
            [FromRoute, Required] Guid id
        )
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                    return NotFound($"Не найден курс по id: {id}");

                return Ok(course);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCourseById(
            [FromRoute] Guid id
        )
        {
            var resultDeleted = await _courseService.DeleteCourseAsync(id);
            if (!resultDeleted)
                return NotFound($"Не удалось удалить курс по id: {id}");

            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseDto>> UpdateCourse(
            [FromRoute] Guid id,
            [FromBody] UpdateCourseDto updateCourseDto
        )
        {
            var courseUpdated = await _courseService.UpdateCourseAsync(id, updateCourseDto);
            if (courseUpdated == null)
                return NotFound($"Не удалось обновить курс по id: {id}");

            return Ok(courseUpdated);
        }

        // [HttpPatch("{id}")]
        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // public IActionResult PartialUpdateCourse(
        //     [FromRoute] Guid id,
        //     [FromBody] UpdateCourseTestRequest request
        // )
        // {
        //     var course = GetCourseFromCursesById(id);
        //     if (course == null)
        //     {
        //         return NotFound($"Курс с id: {id} не найден!");
        //     }
        //     if (!string.IsNullOrEmpty(request.Title))
        //         course.Title = request.Title;
        //     if (!string.IsNullOrEmpty(request.Description))
        //         course.Description = request.Description;
        //     if (!string.IsNullOrEmpty(request.Tags))
        //         course.Tags = request.Tags;
        //     return NoContent();
        // }
    }
}
