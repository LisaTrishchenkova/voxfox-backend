namespace VoxFox.Models.Entities
{
    public class Section
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public Guid CourseId { get; set; }

        public Course Course { get; set; } = null!;
         public ICollection<Lesson> Lessons { get; set; }
    }
}
