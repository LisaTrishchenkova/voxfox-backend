using VoxFox.Enums;

namespace VoxFox.Models.DTOs;

public class CourseDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public CourseStatus Status { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime PublishedAt { get; set; }
    public AuthorDto? Author { get; set; }
    public ICollection<TagDto>? Tags { get; set; } = new List<TagDto>();
}