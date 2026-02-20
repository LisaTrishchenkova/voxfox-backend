using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.DTOs
{
    public class CreateSectionDto
    {
        [Required]
        [MinLength(2)]
        public string Title { get; set; } = null!;

        [Required]
        [MinLength(10)]
        public string Description { get; set; } = null!;
    }
}
