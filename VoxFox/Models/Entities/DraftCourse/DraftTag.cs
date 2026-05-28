namespace VoxFox.Models.Entities;

public class DraftTag
{
    public Guid Id { get; set; }
    public Guid DraftId { get; set; }
    public CourseDraft Draft { get; set; } = null!;
    public string Name { get; set; } = null!;
}
