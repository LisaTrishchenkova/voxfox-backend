namespace VoxFox.Models.DTOs;

public class ReorderLessonsDto
{
    public Guid SectionId { get; set; }
    public List<Guid> LessonIds { get; set; } = new();
}
