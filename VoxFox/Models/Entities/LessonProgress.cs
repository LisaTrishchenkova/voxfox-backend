namespace VoxFox.Models.Entities;

public class LessonProgress
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public Guid EnrollmentId { get; set; }
    public DateTime CompletedAt { get; set; }

    public User? User { get; set; }
    public Lesson? Lesson { get; set; }
    public Enrollment? Enrollment { get; set; }
}
