using VoxFox.Enums;

namespace VoxFox.Models.Entities;

public class DraftTask
{
    public Guid Id { get; set; }
    public Guid DraftLessonId { get; set; }
    public DraftLesson DraftLesson { get; set; } = null!;
    public Guid? OriginalTaskId { get; set; }
    public TaskType Type { get; set; }
    public string Question { get; set; } = null!;
    public List<string>? Options { get; set; }
    public int? CorrectIndex { get; set; }
    public List<int>? CorrectIndexes { get; set; }
    public string? CorrectAnswer { get; set; }
    public string? Explanation { get; set; }
    public int Points { get; set; } = 1;
    public bool IsRequired { get; set; } = true;
    public int OrderIndex { get; set; }
}
