using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Enums;
using VoxFox.Models.DTOs;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ICourseService courseService, ILogger<CoursesController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<CourseDto>>> Search(
            [FromQuery] string? searchTerm,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] CoursesSortBy? sortBy = CoursesSortBy.Relevance)
        {
            try
            {
                if (page < 1)
                {
                    return BadRequest(new { error = "Page должен быть больше или равен 1" });
                }

                if (pageSize < 1 || pageSize > 50)
                {
                    return BadRequest(new { error = "PageSize должен быть от 1 до 50" });
                }

                // if (!Enum.TryParse<CoursesSortBy>(sortBy, true, out var sortByEnum))
                // {
                //     return BadRequest(new
                //     {
                //         error = "Недопустимое значение sortBy. Допустимые значения: relevance, price, title"
                //     });
                // }

                var request = new CourseSearchRequest
                {
                    SearchTerm = searchTerm,
                    Page = page,
                    PageSize = pageSize,
                    CategoryId = categoryId,
                    SortBy = sortBy
                };

                var result = await _courseService.SearchAsync(request);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске курсов");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPut("{id}/publish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PublishCourse([FromRoute] Guid id)
        {
            var result = await _courseService.PublishCourseAsync(id);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
            }

            return NoContent();
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
                return StatusCode(500, ex.StackTrace);
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

        [HttpGet("{courseId}/sections")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SectionDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SectionDto>>> GetSectionsByCourseId(
            [FromRoute, Required] Guid courseId
        )
        {
            try
            {
                var result = await _courseService.GetSectionsByCourseIdAsync(courseId);
                if (!result.Success)
                {
                    return StatusCode(result.StatusCode ?? 400, result.Message);
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
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
