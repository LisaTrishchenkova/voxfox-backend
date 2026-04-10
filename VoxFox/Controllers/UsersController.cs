using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Extensions;
using VoxFox.Interfaces.User;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;
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
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
        public async Task<IActionResult> GetUserById(
            [FromRoute] Guid id
        )
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
            }
            var userResponse = new UserResponse
            {
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            };
            return Ok(userResponse);
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetRole
        (
	        [FromRoute] Guid id,
	        [FromBody] SetRoleRequest request
        )
        {
	        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
	        if (user == null)
		        return NotFound("Пользователь не найден");

	        if (!Enum.IsDefined(typeof(UserRole), request.Role))
		        return BadRequest("Недопустимая роль");

	        user.Role = request.Role;
	        await _context.SaveChangesAsync();

	        return NoContent();
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
    }
}
