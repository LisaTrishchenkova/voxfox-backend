using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Extensions;
using VoxFox.Interfaces.User;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;
using VoxFox.Models.Requests.UserRequest;
using VoxFox.Models.Responses.UserResponse;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IFileStorageService _fileStorageService;

    public UsersController(ApplicationContext context, IFileStorageService fileStorageService)
        {
	        _context = context;
	        _fileStorageService = fileStorageService;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
        public async Task<IActionResult> GetUserById(
            [FromRoute] Guid id
        )
        {
	        var currentUserRole = User.GetUserRole();

	        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

	        if (user == null)
		        return NotFound("Пользователь не найден");

	        var userResponse = new UserResponse
	        {
		        Id = user.Id,
		        Name = user.Name,
		        Email = user.Email,
		        AvatarUrl = user.AvatarUrl,
		        Bio = user.Bio,
		        Role = user.Role.ToString(),
		        CreatedAt = user.CreatedAt,
		        IsDeleted = user.IsDeleted
	        };
	        return Ok(userResponse);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers(
	        [FromQuery] string? search,
	        [FromQuery] UserRole? role,
	        [FromQuery] bool includeDeleted = false,
	        [FromQuery] int page = 1,
	        [FromQuery] int pageSize = 20)
        {
	        var query = includeDeleted
		        ? _context.Users.IgnoreQueryFilters().AsQueryable()
		        : _context.Users.AsQueryable();

	        if (!string.IsNullOrWhiteSpace(search))
		        query = query.Where(u =>
			        u.Name.Contains(search) ||
			        u.Email.Contains(search));

	        if (role.HasValue)
		        query = query.Where(u => u.Role == role.Value);

	        var totalCount = await query.CountAsync();

	        var users = await query
		        .OrderBy(u => u.CreatedAt)
		        .Skip((page - 1) * pageSize)
		        .Take(pageSize)
		        .Select(u => new UserListItemDto
		        {
			        Id = u.Id,
			        Name = u.Name,
			        Email = u.Email,
			        Role = u.Role.ToString(),
			        AvatarUrl = u.AvatarUrl,
			        CreatedAt = u.CreatedAt,
			        IsDeleted = u.IsDeleted
		        })
		        .ToListAsync();

	        return Ok(new
	        {
		        items = users,
		        totalCount,
		        currentPage = page,
		        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
		        pageSize
	        });
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetRole(
	        [FromRoute] Guid id,
	        [FromQuery] UserRole role)
        {
	        var currentUserId = User.GetUserId();

	        if (currentUserId == id)
		        return BadRequest(new { error = "Нельзя изменить роль самому себе" });

	        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
	        if (user == null)
		        return NotFound("Пользователь не найден");

	        var adminCount = await _context.Users
		        .CountAsync(u => u.Role == UserRole.Admin);
	        if (user.Role == UserRole.Admin && role != UserRole.Admin && adminCount <= 1)
		        return BadRequest(new { error = "Нельзя понизить последнего администратора" });

	        user.Role = role;
	        await _context.SaveChangesAsync();

	        return NoContent();
        }

        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
	        var userId = User.GetUserId();
	        if (userId == null) return Unauthorized();

	        if (request.Name == null && request.Bio == null)
		        return BadRequest(new { error = "Необходимо указать хотя бы одно поле для обновления" });

	        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
	        if (user == null)
		        return NotFound(new { error = "Пользователь не найден" });

	        if (request.Name != null)
		        user.Name = request.Name;

	        if (request.Bio != null)
		        user.Bio = request.Bio;

	        await _context.SaveChangesAsync();

	        return NoContent();
        }

        [HttpPut("password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
	        var userId = User.GetUserId();
	        if (userId == null) return Unauthorized();

	        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
	        if (user == null)
		        return NotFound(new { error = "Пользователь не найден" });

	        if (user.Password != request.OldPassword)
		        return BadRequest(new { error = "Неверный текущий пароль" });

	        user.Password = request.NewPassword;
	        await _context.SaveChangesAsync();

	        return NoContent();
        }

        [HttpGet("{id}/courses")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserCourses([FromRoute] Guid id)
        {
	        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
	        if (user == null)
		        return NotFound(new { error = "Пользователь не найден" });

	        var currentUserId = User.GetUserId(); // null если аноним
	        var currentUserRole = User.GetUserRole(); // null если аноним

	        var isPrivileged = currentUserId == id ||
	                           currentUserRole == UserRole.Admin ||
	                           currentUserRole == UserRole.Moderator;

	        var query = _context.Courses
		        .Where(c => c.AuthorId == id && !c.IsDeleted);

	        if (!isPrivileged)
		        query = query.Where(c => c.Status == CourseStatus.Published);

	        var courses = await query
		        .Include(c => c.Author)
		        .Include(c => c.Tags)
		        .OrderByDescending(c => c.CreatedAt)
		        .Select(c => new CourseDto
		        {
			        Id = c.Id,
			        Title = c.Title,
			        Description = c.Description,
			        FullDescription = c.FullDescription,
			        Status = c.Status,
			        Level = c.Level,
			        CoverImageUrl = c.CoverImageUrl,
			        Price = c.Price,
			        CertificateEnabled = c.CertificateEnabled,
			        EnrollmentCount = c.EnrollmentCount,
			        Rating = c.Rating,
			        DurationMinutes = c.DurationMinutes,
			        CategoryId = c.CategoryId,
			        CreatedAt = c.CreatedAt,
			        PublishedAt = c.PublishedAt,
			        Author = new AuthorDto { Id = c.Author!.Id, Name = c.Author.Name },
			        Tags = c.Tags != null
				        ? c.Tags.Select(t => new TagDto { Name = t.Name }).ToList()
				        : new List<TagDto>()
		        })
		        .ToListAsync();

	        return Ok(courses);
        }

        [HttpGet("{id}/stats")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserStatsDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserStats([FromRoute] Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(new { error = "Пользователь не найден" });

            var stats = new UserStatsDto();

            if (user.Role == UserRole.Student)
            {
                var enrollments = await _context.Enrollments
                    .Where(e => e.UserId == id)
                    .ToListAsync();

                stats.EnrolledCoursesCount = enrollments.Count;
                stats.CompletedCoursesCount = enrollments
                    .Count(e => e.Status == EnrollmentStatus.Completed);
                stats.InProgressCoursesCount = enrollments
                    .Count(e => e.Status == EnrollmentStatus.Active);
                stats.TotalScore = await _context.TaskSubmissions
                    .Where(s => s.UserId == id && s.IsCorrect == true)
                    .SumAsync(s => s.Score);
            }
            else if (user.Role == UserRole.Teacher)
            {
                var courseIds = await _context.Courses
                    .Where(c => c.AuthorId == id && !c.IsDeleted)
                    .Select(c => c.Id)
                    .ToListAsync();

                stats.CreatedCoursesCount = courseIds.Count;
                stats.PublishedCoursesCount = await _context.Courses
                    .CountAsync(c => c.AuthorId == id &&
                                     !c.IsDeleted &&
                                     c.Status == CourseStatus.Published);
                stats.TotalStudentsCount = await _context.Enrollments
                    .CountAsync(e => courseIds.Contains(e.CourseId));
                stats.AverageRating = await _context.Courses
                    .Where(c => c.AuthorId == id &&
                                !c.IsDeleted &&
                                c.Status == CourseStatus.Published)
                    .AverageAsync(c => (double?)c.Rating) ?? 0;
            }

            return Ok(stats);
        }

        [HttpPost("avatar")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
	        var userId = User.GetUserId();
	        if (userId == null) return Unauthorized();

	        if (file == null || file.Length == 0)
		        return BadRequest(new { error = "Файл не выбран" });

	        if (file.Length > 5 * 1024 * 1024)
		        return BadRequest(new { error = "Файл не должен превышать 5MB" });

	        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
	        if (user == null)
		        return NotFound(new { error = "Пользователь не найден" });

	        var result = await _fileStorageService.SaveAvatarAsync(userId.Value, file);
	        if (!result.Success)
		        return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

	        user.AvatarUrl = result.Data;
	        await _context.SaveChangesAsync();

	        return Ok(new { avatarUrl = result.Data });
        }

        [HttpDelete("avatar")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAvatar()
        {
	        var userId = User.GetUserId();
	        if (userId == null) return Unauthorized();

	        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
	        if (user == null)
		        return NotFound(new { error = "Пользователь не найден" });

	        if (string.IsNullOrEmpty(user.AvatarUrl))
		        return NotFound(new { error = "Аватарка не установлена" });

	        var result = await _fileStorageService.DeleteAvatarAsync(user.AvatarUrl);
	        if (!result.Success)
		        return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

	        user.AvatarUrl = null;
	        await _context.SaveChangesAsync();

	        return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
	        var currentUserId = User.GetUserId();

	        if (currentUserId == id)
		        return BadRequest(new { error = "Нельзя удалить самого себя" });

	        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
	        if (user == null)
		        return NotFound(new { error = "Пользователь не найден" });

	        if (user.Role == UserRole.Admin)
	        {
		        var adminCount = await _context.Users
			        .CountAsync(u => u.Role == UserRole.Admin);
		        if (adminCount <= 1)
			        return BadRequest(new { error = "Нельзя удалить последнего администратора" });
	        }

	        user.IsDeleted = true;
	        await _context.SaveChangesAsync();

	        return NoContent();
        }

        [HttpPut("{id}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreUser([FromRoute] Guid id)
        {
	        var user = await _context.Users
		        .IgnoreQueryFilters()
		        .FirstOrDefaultAsync(u => u.Id == id);

	        if (user == null)
		        return NotFound(new { error = "Пользователь не найден" });

	        if (!user.IsDeleted)
		        return BadRequest(new { error = "Пользователь не удалён" });

	        user.IsDeleted = false;
	        await _context.SaveChangesAsync();

	        return NoContent();
        }
    }
}
