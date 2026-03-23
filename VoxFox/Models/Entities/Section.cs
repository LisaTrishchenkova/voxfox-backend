using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Entities
{
    public class Section
    {
        public Guid Id { get; init; }
        public string Title { get; set; } = null!;
        [MaxLength(2000)]
        public string Description { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public Guid CourseId { get; init; }

        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public Course Course { get; set; } = null!;
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public ICollection<Lesson> Lessons { get; set; } = null!;
    }
}
