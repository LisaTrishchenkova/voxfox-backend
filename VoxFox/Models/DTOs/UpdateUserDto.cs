using System.ComponentModel.DataAnnotations;
namespace VoxFox.Models.DTOs
{
    public class UpdateUserDto
    {
        [MinLength(2)]
        public string? Name { get; set; } = null!;

        [MinLength(5)]
        public string? Email { get; set; } = null!;
    }
}
