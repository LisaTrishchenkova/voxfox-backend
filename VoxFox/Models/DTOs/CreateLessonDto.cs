using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.DTOs
{
    public class CreateLessonDto
    {
        [Required]
        [MinLength(2)]
        public string Title { get; set; } = null!;

        [Required]
        [MinLength(10)]
        public string Description { get; set; } = null!;

        public string Content { get; set; } = null!;
    }
}