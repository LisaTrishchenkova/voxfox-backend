using VoxFox.Models.Entities;
using VoxFox.Models.DTOs;
using VoxFox.Enums;
public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public CourseStatus Status { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime PublishedAt { get; set; }
    public AuthorDto Author { get; set; }
    public ICollection<TagDto>? Tags { get; set; } = new List<TagDto>();
}
