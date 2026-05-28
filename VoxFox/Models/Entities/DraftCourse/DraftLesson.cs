namespace VoxFox.Models.Entities;

public class DraftLesson
{
    public Guid Id { get; set; }
    public Guid DraftSectionId { get; set; }
    public DraftSection DraftSection { get; set; } = null!;
    public Guid? OriginalLessonId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Content { get; set; }
    public int OrderIndex { get; set; }
    public ICollection<DraftTask> Tasks { get; set; } = [];
}
