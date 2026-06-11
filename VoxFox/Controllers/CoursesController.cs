using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Enums;
using VoxFox.Extensions;
using VoxFox.Interfaces;
using VoxFox.Interfaces.User;
using VoxFox.Models.DTOs;
using VoxFox.Models.Requests;
using VoxFox.Models.Requests.CourseRequest;
using VoxFox.Models.Responses;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ILogger<CoursesController> _logger;
    private readonly IFileStorageService _fileStorageService;

    public CoursesController(ICourseService courseService, ILogger<CoursesController> logger, IFileStorageService fileStorageService)
    {
        _courseService = courseService;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<CourseDto>>> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] CoursesSortBy? sortBy = CoursesSortBy.Relevance,
        [FromQuery] CourseLevel? level = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? isFree = null,
        [FromQuery] CourseStatus? status = null,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] bool onlyDeleted = false)
    {
        _logger.LogInformation("Контроллер получил: minPrice={MinPrice}, maxPrice={MaxPrice}", minPrice, maxPrice);
        try
        {
            if (page < 1) return BadRequest(new { error = "Page должен быть больше или равен 1" });
            if (pageSize < 1 || pageSize > 50) return BadRequest(new { error = "PageSize должен быть от 1 до 50" });
            if (minPrice.HasValue && minPrice < 0) return BadRequest(new { error = "MinPrice не должен быть отрицательным" });
            if (maxPrice.HasValue && maxPrice < 0) return BadRequest(new { error = "MaxPrice не должен быть отрицательным" });
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice) return BadRequest(new { error = "MinPrice не должен быть больше MaxPrice" });

            // includeDeleted доступен только админам
            if (includeDeleted && !User.IsInRole("Admin"))
                includeDeleted = false;

            var request = new CourseSearchRequest
            {
                SearchTerm = searchTerm,
                Page = page,
                PageSize = pageSize,
                CategoryId = categoryId,
                SortBy = sortBy,
                Level = level,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                IsFree = isFree,
                Status = status,
                IncludeDeleted = includeDeleted,
                OnlyDeleted = onlyDeleted
            };

            var result = await _courseService.SearchAsync(request);
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске курсов");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<ActionResult<PaginatedResponse<CourseDto>>> GetPendingCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) return BadRequest(new { error = "Page должен быть больше или равен 1" });
        if (pageSize < 1 || pageSize > 50) return BadRequest(new { error = "PageSize должен быть от 1 до 50" });
        var result = await _courseService.GetPendingCoursesAsync(page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,Admin,Moderator")]
    public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto createCourseDto)
    {
        try
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();
            var course = await _courseService.CreateCourseAsync(createCourseDto, userId.Value);
            return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании курса");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto>> GetCourseById([FromRoute][Required] Guid id)
    {
        try
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound($"Не найден курс по id: {id}");
            return Ok(course);
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> DeleteCourseById(
        [FromRoute] Guid id,
        [FromQuery] string? reason = null)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var isAdmin = User.IsInRole("Admin");
        var resultDeleted = await _courseService.DeleteCourseAsync(id, userId.Value, isAdmin, reason);
        if (!resultDeleted) return NotFound($"Не удалось удалить курс по id: {id}");
        return NoContent();
    }

    [HttpPut("{id}/restore")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreCourse([FromRoute] Guid id)
    {
        var result = await _courseService.RestoreCourseAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher,Admin,Moderator")]
    public async Task<ActionResult<CourseDto>> UpdateCourse(
        [FromRoute] Guid id,
        [FromBody] UpdateCourseDto updateCourseDto)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _courseService.UpdateCourseAsync(id, updateCourseDto, userId.Value);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return Ok(result.Data);
    }

    [HttpGet("{courseId}/sections")]
    public async Task<ActionResult<IEnumerable<SectionDto>>> GetSectionsByCourseId([FromRoute][Required] Guid courseId)
    {
        try
        {
            var result = await _courseService.GetSectionsByCourseIdAsync(courseId);
            if (!result.Success) return StatusCode(result.StatusCode ?? 400, result.Message);
            return Ok(result.Data);
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, $"Ошибка сервера: {ex.Message}");
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<IList<CourseDto>>> GetMyCourses([FromQuery] CourseStatus? status = null)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        var result = await _courseService.GetMyCoursesAsync(userId.Value, status);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return Ok(result.Data);
    }

    [HttpPut("{id}/moderate")]
    [Authorize(Roles = "Teacher,Admin,Moderator")]
    public async Task<IActionResult> ModerateCourse([FromRoute] Guid id)
    {
        var result = await _courseService.ModeratorCourseAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> ApproveCourse([FromRoute] Guid id)
    {
        var result = await _courseService.ApproveCourseAsync(id);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Moderator,Admin")]
    public async Task<IActionResult> RejectCourse(
        [FromRoute] Guid id,
        [FromBody] RejectCourseRequest request)
    {
        var result = await _courseService.RejectCourseAsync(id, request.Reason);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }

    [HttpPut("{id}/unpublish")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnpublishCourse(
        [FromRoute] Guid id,
        [FromBody] UnpublishCourseRequest? request)
    {
        var result = await _courseService.UnpublishCourseAsync(id, request?.Reason);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
        return NoContent();
    }

    [HttpPost("{id}/cover")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> UploadCourseCover([FromRoute] Guid id, IFormFile file)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        if (file == null || file.Length == 0) return BadRequest(new { error = "Файл не выбран" });
        if (file.Length > 10 * 1024 * 1024) return BadRequest(new { error = "Файл не должен превышать 10MB" });

        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null) return NotFound(new { error = "Курс не найден" });

        var isAdmin = User.IsInRole("Admin");
        if (course.Author?.Id != userId.Value && !isAdmin) return Forbid();

        if (!string.IsNullOrEmpty(course.CoverImageUrl) && course.CoverImageUrl.StartsWith("/covers/"))
            await _fileStorageService.DeleteCourseCoverAsync(course.CoverImageUrl);

        var result = await _fileStorageService.SaveCourseCoverAsync(id, file);
        if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        var updateDto = new UpdateCourseDto { CoverImageUrl = result.Data };
        await _courseService.UpdateCourseAsync(id, updateDto, userId.Value);
        return Ok(new { coverImageUrl = result.Data });
    }

    [HttpDelete("{id}/cover")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> DeleteCourseCover([FromRoute] Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null) return NotFound(new { error = "Курс не найден" });

        var isAdmin = User.IsInRole("Admin");
        if (course.Author?.Id != userId.Value && !isAdmin) return Forbid();
        if (string.IsNullOrEmpty(course.CoverImageUrl)) return NotFound(new { error = "Обложка не установлена" });

        if (course.CoverImageUrl.StartsWith("/covers/"))
        {
            var deleteResult = await _fileStorageService.DeleteCourseCoverAsync(course.CoverImageUrl);
            if (!deleteResult.Success) return StatusCode(deleteResult.StatusCode ?? 400, new { error = deleteResult.Message });
        }

        var updateDto = new UpdateCourseDto { CoverImageUrl = null };
        await _courseService.UpdateCourseAsync(id, updateDto, userId.Value);
        return NoContent();
    }
}
