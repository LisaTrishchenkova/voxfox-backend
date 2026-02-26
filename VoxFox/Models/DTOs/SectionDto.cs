namespace VoxFox.Models.DTOs
{
    public class SectionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
