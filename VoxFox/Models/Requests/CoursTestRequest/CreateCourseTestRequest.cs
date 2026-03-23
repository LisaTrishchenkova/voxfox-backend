using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Requests.CoursTestRequest
{
    public class CreateCourseTestRequest
    {
        [Required]
        [MinLength(2)]
        public required string Title { get; set; }

        [Required]
        [MinLength(10)]
        public required string Description { get; set; }

        [MinLength(8)]
        public string? Tags { get; set; }
    }
}
