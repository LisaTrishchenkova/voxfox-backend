using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Requests
{
    public class LoginRequest()
    {
        /// <summary>
        /// Email используемый для входа
        /// </summary>
        /// <value></value>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Пароль используемый для входа
        /// </summary>
        /// <value></value>
        [Required]
        [MinLength(8)]
        public string Password { get; set; }

    }

}
