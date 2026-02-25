using System.ComponentModel.DataAnnotations;
namespace VoxFox.Models.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = null!;

        [Required]
        [MinLength(5)]
        public string Email { get; set; } = null!;
    }
}
