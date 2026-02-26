namespace VoxFox.Models.Entities
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsDeleted { get; set; } = false; // TODO: удалить после просмотра!

        public ICollection<Tag>? Tags { get; set; } = null!;
        public ICollection<Section> Sections { get; set; } = null!;
    }
}
