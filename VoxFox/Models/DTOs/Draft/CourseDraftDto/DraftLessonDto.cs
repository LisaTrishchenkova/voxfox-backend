namespace VoxFox.Models.DTOs.Draft.CourseDraftDto;

public class DraftLessonDto
{
    public Guid Id { get; set; }
    public Guid? OriginalLessonId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Content { get; set; }
    public int OrderIndex { get; set; }
    public List<DraftTaskDto> Tasks { get; set; } = [];
}
