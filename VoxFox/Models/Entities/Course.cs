using VoxFox.Enums;

namespace VoxFox.Models.Entities
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public CourseStatus Status { get; set; }
        public Guid? CategoryId { get; set; }

        public Guid? AuthorId { get; set; }
        public Author? Author { get; set; }
        // public DateTime PublishedAt { get; set; }


        public ICollection<Tag>? Tags { get; set; } = null!;
        public ICollection<Section> Sections { get; set; } = null!;
        public Category? Category { get; set; } = null!;
    }
}
