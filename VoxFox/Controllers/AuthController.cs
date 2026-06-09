using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces;
using VoxFox.Models.Entities;
using VoxFox.Models.Requests.AuthRequest;
using VoxFox.Models.Responses.AuthResponse;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly ApplicationContext _applicationContext;

        public AuthController(IJwtService jwtService, ApplicationContext applicationContext)
        {
            _jwtService = jwtService;
            _applicationContext = applicationContext;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // IgnoreQueryFilters — чтобы найти удалённых пользователей
            var user = _applicationContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefault(u => u.Email == request.Email && u.Password == request.Password);

            if (user == null)
                return BadRequest(new { code = "INVALID_CREDENTIALS", message = "Неверный email или пароль" });

            if (user.IsDeleted)
                return StatusCode(403, new { code = "ACCOUNT_DELETED", message = "Ваш аккаунт был удалён" });

            if (user.IsBlocked)
                return StatusCode(403, new
                {
                    code = "ACCOUNT_BLOCKED",
                    message = "Ваш аккаунт заблокирован",
                    reason = user.BlockReason
                });

            var claims = _jwtService.CreateClaims(user.Id, request.Email, user.Role);
            var accessToken = _jwtService.GenerateAccessToken(claims);
            var loginResponse = new LoginResponse
            {
                TokenAccess = accessToken,
                TokenRefresh = "",
                UserId = user.Id,
            };
            return Ok(loginResponse);
        }

        [HttpPost("registration")]
        public IActionResult Registration([FromBody] RegistrationRequest request)
        {
            var existingUser = _applicationContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefault(u => u.Email == request.Email);

            if (existingUser != null)
            {
                // удалённый пользователь пытается зарегистрироваться заново
                if (existingUser.IsDeleted)
                    return StatusCode(403, new { code = "ACCOUNT_DELETED", message = "Аккаунт с этим email был удалён. Обратитесь в поддержку." });

                return Conflict(new { code = "EMAIL_TAKEN", message = "Пользователь с таким email уже существует" });
            }

            var allowedRoles = new[] { UserRole.Student, UserRole.Teacher };
            if (!allowedRoles.Contains(request.Role))
                return BadRequest(new { code = "INVALID_ROLE", message = "Недопустимая роль" });

            _applicationContext.Users.Add(new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                Role = request.Role
            });
            _applicationContext.SaveChanges();

            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RefreshResponse))]
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            return Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MeResponse))]
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = _applicationContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound();

            return Ok(new MeResponse
            {
                Id = user.Id,
                Name = user.Name,
                Bio = user.Bio,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                AvatarUrl = user.AvatarUrl
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            return Ok();
        }
    }
}
