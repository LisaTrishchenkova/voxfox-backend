using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Entities
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Tags { get; set; }
    }
}
