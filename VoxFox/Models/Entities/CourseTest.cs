using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Entities
{
    public class CourseTest
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        public string? Tags { get; set; }
    }
}
