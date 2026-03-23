using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("{id}/test")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
        public async Task<IActionResult> GetTestUserById(
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
    }
}
