using System.ComponentModel.DataAnnotations;
using VoxFox.Enums;

namespace VoxFox.Models.Requests.AuthRequest
{
    public class RegistrationRequest
    {
        [Required]
        [MinLength(2)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(8)]
        public required string Password { get; set; }

        public UserRole Role { get; set; } = UserRole.Student;
    }
}
