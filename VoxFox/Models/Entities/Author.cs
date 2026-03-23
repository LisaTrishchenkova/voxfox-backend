namespace VoxFox.Models.Entities
{
    public class Author
    {
        public Guid Id { get; init; }
        public string Name { get; set; } = null!;
        // ReSharper disable once PropertyCanBeMadeInitOnly.Global
        public ICollection<Course>? Courses { get; set; }
    }
}
