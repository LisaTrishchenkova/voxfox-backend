namespace VoxFox.Models.DTOs;

public class LessonProgressDto
{
    public Guid LessonId { get; set; }
    public Guid EnrollmentId { get; set; }
    public DateTime CompletedAt { get; set; }
    public int ProgressPercent { get; set; }
    public List<NewAchievementDto> NewAchievements { get; set; } = new();
}
