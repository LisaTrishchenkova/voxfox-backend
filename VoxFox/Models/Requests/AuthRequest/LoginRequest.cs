using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Requests.AuthRequest
{
    public class LoginRequest
    {
        /// <summary>
        /// Email используемый для входа
        /// </summary>
        /// <value></value>
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        /// <summary>
        /// Пароль используемый для входа
        /// </summary>
        /// <value></value>
        [Required]
        [MinLength(8)]
        public required string Password { get; set; }

    }

}
