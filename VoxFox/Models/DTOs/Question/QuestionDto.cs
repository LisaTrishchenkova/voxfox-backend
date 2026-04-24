namespace VoxFox.Models.DTOs.Question;

public class QuestionDto
{
	public Guid Id { get; set; }
	public Guid LessonId { get; set; }
	public Guid AuthorId { get; set; }
	public string? AuthorName { get; set; }
	public string Text { get; set; } = null!;
	public string? AnswerText { get; set; }
	public string? AnsweredByName { get; set; }
	public DateTime? AnsweredAt { get; set; }
	public DateTime CreatedAt { get; set; }
	public bool IsAnswered => AnswerText != null;
}
