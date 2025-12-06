using Microsoft.AspNetCore.Mvc;
using VoxFox.Models.Requests;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        // ".../api/auth/login" 
        [HttpPost("login")]
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
    }
}