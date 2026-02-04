using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Entities
{
    public class CourseTest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;

        public string? Tags { get; set; }
    }
}
