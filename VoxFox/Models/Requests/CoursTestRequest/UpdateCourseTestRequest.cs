using System.ComponentModel.DataAnnotations;
namespace VoxFox.Models.Requests
{
    public class UpdateCourseTestRequest
    {
        [MinLength(2)]
        public string? Title { get; set; }

        [MinLength(10)]
        public string? Description { get; set; }

        [MinLength(8)]
        public string? Tags { get; set; }
    }
}
