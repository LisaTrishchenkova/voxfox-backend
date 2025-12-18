using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxFox.Models.Entities;
using VoxFox.Models.Requests;
using VoxFox.Models.Responses;
using static System.Net.WebRequestMethods;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {

        private readonly ApplicationContext _context;

        public CoursesController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CourseResponse>))]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _context.Courses
                .Where(c => c.IsActive)
                .ToListAsync();

            var response = courses.Select(MapToResponse).ToList();

            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourseResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCourseById(
            [FromRoute] Guid id
        )
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound("Курс не найден!");
            }

            return Ok(MapToResponse(course));
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateCourse(
            [FromBody] CreateCourseRequest request
        )
        {
            var userId = GetCurrentUserId();
            var course = new Course
            {
                Title = request.Title,
                Description = request.Description,
                ShortDescription = request.ShortDescription,
                Category = request.Category,
                Level = request.Level,
                ImageUrl = request.ImageUrl,
                LessonsCount = request.LessonsCount,
                Duration = request.Duration,
                Format = request.Format,
                HasCertificate = request.HasCertificate,
                HasHomework = request.HasHomework,
                IsPaid = request.IsPaid,
                Price = request.Price,
                DiscountedPrice = request.DiscountedPrice,
                Tags = request.Tags != null ? string.Join(",", request.Tags) : null,
                Status = "draft",
                AuthorId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);

            _context.SaveChanges();

            return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, null);
        }

        [HttpPatch("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateCourse(
            [FromRoute] Guid id,
            [FromBody] UpdateCourseRequest request
        )
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound("Курс не найден");
            }

            var userId = GetCurrentUserId();
            if (course.AuthorId != userId)
            {
                return Forbid("Вы не являетесь автором этого курса");
            }

            if (!string.IsNullOrEmpty(request.Title))
                course.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Description))
                course.Description = request.Description;
            if (request.ShortDescription != null)
                course.ShortDescription = request.ShortDescription;
            if (!string.IsNullOrEmpty(request.Category))
                course.Category = request.Category;
            if (!string.IsNullOrEmpty(request.Level))
                course.Level = request.Level;
            if (request.ImageUrl != null)
                course.ImageUrl = request.ImageUrl;
            if (request.LessonsCount.HasValue)
                course.LessonsCount = request.LessonsCount.Value;
            if (!string.IsNullOrEmpty(request.Duration))
                course.Duration = request.Duration;
            if (!string.IsNullOrEmpty(request.Format))
                course.Format = request.Format;
            if (request.HasCertificate.HasValue)
                course.HasCertificate = request.HasCertificate.Value;
            if (request.HasHomework.HasValue)
                course.HasHomework = request.HasHomework.Value;
            if (request.IsPaid.HasValue)
                course.IsPaid = request.IsPaid.Value;
            if (request.Price.HasValue)
                course.Price = request.Price.Value;
            if (request.DiscountedPrice.HasValue)
                course.DiscountedPrice = request.DiscountedPrice.Value;
            if (request.Tags != null)
                course.Tags = string.Join(",", request.Tags);
            if (!string.IsNullOrEmpty(request.Status))
                course.Status = request.Status;

            course.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult DeleteCourse(
            [FromRoute] Guid id
        )
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound("Курс не найден");
            }

            var userId = GetCurrentUserId();
            if (course.AuthorId != userId)
            {
                return Forbid("Вы не являетесь автором этого курса");
            }

            course.IsActive = false;
            course.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();
            return NoContent();
        }


        [HttpPost("{id}/publish")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult PublishCourse(
            [FromRoute] Guid id
        )
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id && c.IsActive);
            if (course == null)
            {
                return NotFound("Курс не найден");
            }

            var userId = GetCurrentUserId();
            if (course.AuthorId != userId)
            {
                return Forbid("Вы не являетесь автором этого курса");
            }

            course.Status = "published";
            course.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            return NoContent();
        }

        [HttpGet("my")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CourseResponse>))]
        public IActionResult GetMyCourses()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Требуется авторизация");
            }

            var courses = _context.Courses
                .Include(c => c.Author)
                .Where(c => c.AuthorId == userId && c.IsActive)
                .OrderByDescending(c => c.UpdatedAt)
                .ToList();

            var response = courses.Select(MapToResponse).ToList();
            return Ok(response);
        }


        // [HttpGet("search")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CourseResponse>))]
        // public IActionResult SearchCourses(
        //     [FromQuery] string title
        // )
        // {
        //     // Ищем курсы, где название ИЛИ описание содержит искомый текст
        //     // .Where() - фильтрует коллекцию по условию
        //     // c.Title.Contains(title) - проверяет, содержится ли title в Title
        //     // || - логическое ИЛИ (или в Title, или в Description)
        //     var courses = _context.Courses.Where(c => c.Title.Contains(title) || c.Description.Contains(title)).ToList();

        //     var response = courses.Select(c => new CourseResponse
        //     {

        //     }).ToList();
        //     return Ok(response);
        // }

        private Guid GetCurrentUserId()
        {
            // var claims = User.Claims.ToList();
            // var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }

            throw new InvalidOperationException("User ID claim not found");
        }

        private CourseResponse MapToResponse(Course course)
        {
            return new CourseResponse
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                ShortDescription = course.ShortDescription,
                Category = course.Category,
                Level = course.Level,
                ImageUrl = course.ImageUrl,
                LessonsCount = course.LessonsCount,
                Duration = course.Duration,
                Format = course.Format,
                HasCertificate = course.HasCertificate,
                HasHomework = course.HasHomework,
                IsPaid = course.IsPaid,
                Price = course.Price,
                DiscountedPrice = course.DiscountedPrice,
                Tags = course.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Status = course.Status,
                AuthorId = course.AuthorId,
                AuthorName = course.Author?.Name,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                IsActive = course.IsActive
            };
        }
    }
}
