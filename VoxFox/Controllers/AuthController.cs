using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Models.Requests;
using VoxFox.Models.Responses;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

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

            return Ok();
        }

        // ".../api/auth/registration"
        [HttpPost("registration")]
        public IActionResult Registration(
            [FromBody] RegistrationRequest request
        )
        {
            return Ok();
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
