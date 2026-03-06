namespace VoxFox.Models.Entities
{
    public class Lesson
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Content { get; set; }
        public bool IsDeleted { get; set; } = false;
        public Guid SectionId { get; set; }

        public Section Section { get; set; } = null!;
    }
}