namespace VoxFox.Models.Entities;

public class DraftSection
{
    public Guid Id { get; set; }
    public Guid DraftId { get; set; }
    public CourseDraft Draft { get; set; } = null!;
    public Guid? OriginalSectionId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int OrderIndex { get; set; }
    public ICollection<DraftLesson> Lessons { get; set; } = [];
}
