namespace VoxFox.Models.Entities
{
    public class Author
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
         public ICollection<Course> Courses { get; set; }
    }
}
