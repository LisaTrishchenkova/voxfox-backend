using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Interfaces;
using VoxFox.Models.Entities;
using VoxFox.Models.Requests;
using VoxFox.Models.Responses;

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

        /// <summary>
        /// Ручка для входа
        /// </summary>
        /// <param name="request"></param>
        /// <returns>
        ///Ответ ручки для входа
        /// </returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        public IActionResult Login(
            [FromBody] LoginRequest request
            )
        {
            var user = _applicationContext.Users.FirstOrDefault(u => u.Email == request.Email && u.Password == request.Password);
            if (user == null)
            {
                return NotFound("Email или пароль неверный");
            }
            var claims = _jwtService.CreateClaims(user.Id, request.Email);
            var accessToken = _jwtService.GenerateAccessToken(claims);
            var loginResponse = new LoginResponse
            {
                TokenAccess = accessToken,
                TokenRefresh = "",
                UserId = user.Id,
            };
            return Ok(loginResponse);
        }

        // ".../api/auth/registration"
        [HttpPost("registration")]
        public IActionResult Registration(
            [FromBody] RegistrationRequest request
        )
        {
            _applicationContext.Users.Add(new Models.Entities.User
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password
            });
            _applicationContext.SaveChanges();

            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RefreshResponse))]
        [HttpPost("refresh")]
        public IActionResult Refresh(
           [FromBody] RefreshRequest request
       )
        {
            return Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MeResponse))]
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok();
        }

        [HttpPost("logout")]
        public IActionResult Logout(
            [FromBody] LogoutRequest request
        )
        {
            return Ok();
        }

    }

}
