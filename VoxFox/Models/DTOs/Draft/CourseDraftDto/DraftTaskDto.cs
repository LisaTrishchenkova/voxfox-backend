namespace VoxFox.Models.DTOs.Draft.CourseDraftDto;

public class DraftTaskDto
{
    public Guid Id { get; set; }
    public Guid? OriginalTaskId { get; set; }
    public string Type { get; set; } = null!;
    public string Question { get; set; } = null!;
    public List<string>? Options { get; set; }
    public int? CorrectIndex { get; set; }
    public List<int>? CorrectIndexes { get; set; }
    public string? CorrectAnswer { get; set; }
    public string? Explanation { get; set; }
    public int Points { get; set; }
    public bool IsRequired { get; set; }
    public int OrderIndex { get; set; }
}
