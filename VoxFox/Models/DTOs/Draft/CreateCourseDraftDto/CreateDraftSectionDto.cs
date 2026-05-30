namespace VoxFox.Models.DTOs.Draft.CreateCourseDraftDto;

public class CreateDraftSectionDto
{
    public Guid? OriginalSectionId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int OrderIndex { get; set; }
    public List<CreateDraftLessonDto> Lessons { get; set; } = [];
}
