using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Entities
{
    public class Tag
    {
        public Guid Id { get; init; }
        [MaxLength(200)]
        public string Name { get; set; } = null!;
        public Guid CourseId { get; set; }
        
        public Course Course { get; set; } = null!;
    }
}
