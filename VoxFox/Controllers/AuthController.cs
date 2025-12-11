using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Interfaces;
using VoxFox.Models.Requests;
using VoxFox.Models.Responses;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;

        public AuthController(IJwtService jwtService)
        {
            _jwtService = jwtService;
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
            var userId = Guid.NewGuid();
            var claims = _jwtService.CreateClaims(userId, request.Email);
            var accessToken = _jwtService.GenerateAccessToken(claims);
            var loginResponse = new LoginResponse
            {
                TokenAccess = accessToken,
                TokenRefresh = "",
                UserId = userId,
            };
            return Ok(loginResponse);
        }

        // ".../api/auth/registration"
        [HttpPost("registration")]
        public IActionResult Registration(
            [FromBody] RegistrationRequest request
        )
        {

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
