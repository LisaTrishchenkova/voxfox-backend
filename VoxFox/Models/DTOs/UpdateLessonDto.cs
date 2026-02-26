using System.ComponentModel.DataAnnotations;
namespace VoxFox.Models.DTOs
{
    public class UpdateLessonDto
    {
        [MinLength(2)]
        public string? Title { get; set; }

        [MinLength(10)]
        public string? Description { get; set; }

        public string Content {get; set;}
    }
}