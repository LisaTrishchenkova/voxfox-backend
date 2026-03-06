using VoxFox.Models.Entities;
using VoxFox.Models.DTOs;
public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsPublished { get; set; }
    public Guid? CategoryId { get; set; }
    public ICollection<TagDto>? Tags { get; set; } = new List<TagDto>();
}
