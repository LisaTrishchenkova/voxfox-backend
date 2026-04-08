namespace VoxFox.Models.DTOs.Tasks;

public class TaskSubmissionDto
{
	public Guid Id { get; init; }
	public Guid TaskId { get; init; }
	public Guid UserId { get; init; }
	public int? AnswerIndex { get; init; }
	public List<int>? AnswerIndexes { get; init; }
	public string? AnswerText { get; init; }
	public bool? IsCorrect { get; init; }
	public int Score { get; init; }
	public int AttemptNumber { get; init; }
	public DateTime SubmittedAt { get; init; }
	public DateTime? ReviewedAt { get; init; }
}
