namespace VoxFox.Models.Entities
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public bool IsPublished { get; set; } = false;
        public Guid? CategoryId { get; set; }

        public ICollection<Tag>? Tags { get; set; } = null!;
        public ICollection<Section> Sections { get; set; } = null!;
        public Category? Category { get; set; } = null!;
    }
}
