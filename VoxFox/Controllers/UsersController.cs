using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
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

        public UsersController(ApplicationContext context)
        {
            _context = context;
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
                Email = user.Email
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
    }
}
