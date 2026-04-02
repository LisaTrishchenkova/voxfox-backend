using VoxFox.Models.Entities;

namespace VoxFox.Models.DTOs.Tasks;

public class TaskSubmission
{
	public Guid Id { get; set; }
	public Guid TaskId { get; set; }
	public Guid UserId { get; set; }

	public int? AnswerIndex { get; set; }
	public List<int>? AnswerIndexes { get; set; }
	public string? AnswerText { get; set; }

	public bool? IsCorrect { get; set; }
	public int Score { get; set; } = 0;
	public int AttemptNumber { get; set; } = 1;
	public DateTime SubmittedAt { get; set; }
	public DateTime? ReviewedAt { get; set; }

	public TaskEntity? Task { get; set; }
	public User? User { get; set; }
}
