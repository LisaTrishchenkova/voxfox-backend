using System.ComponentModel.DataAnnotations;

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
    }
}