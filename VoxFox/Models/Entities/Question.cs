namespace VoxFox.Models.Entities;

public class Question
{
	public Guid Id { get; set; }
	public Guid LessonId { get; set; }
	public Guid AuthorId { get; set; }
	public string Text { get; set; } = null!;
	public string? AnswerText { get; set; }
	public Guid? AnsweredById { get; set; }
	public DateTime? AnsweredAt { get; set; }
	public DateTime CreatedAt { get; set; }
	public bool IsDeleted { get; set; } = false;

	public Lesson? Lesson { get; set; }
	public User? Author { get; set; }
	public User? AnsweredBy { get; set; }
}
