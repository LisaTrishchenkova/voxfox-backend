namespace VoxFox.Models.DTOs.Draft.CreateCourseDraftDto;

public class CreateDraftLessonDto
{
    public Guid? OriginalLessonId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Content { get; set; }
    public int OrderIndex { get; set; }
    public List<CreateDraftTaskDto> Tasks { get; set; } = [];
}
