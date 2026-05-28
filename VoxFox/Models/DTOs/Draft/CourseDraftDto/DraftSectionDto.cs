namespace VoxFox.Models.DTOs.Draft.CourseDraftDto;

public class DraftSectionDto
{
    public Guid Id { get; set; }
    public Guid? OriginalSectionId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int OrderIndex { get; set; }
    public List<DraftLessonDto> Lessons { get; set; } = [];
}
