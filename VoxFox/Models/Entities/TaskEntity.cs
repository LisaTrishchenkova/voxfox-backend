using VoxFox.Enums;
using VoxFox.Models.Entities;

namespace VoxFox.Models.DTOs.Tasks;

public class TaskEntity
{
	public Guid Id { get; set; }
	public Guid LessonId { get; set; }
	public TaskType Type { get; set; }
	public string Question { get; set; } = null!;

	public List<string>? Options { get; set; }
	public int? CorrectIndex { get; set; }
	public List<int>? CorrectIndexes { get; set; }
	public string? CorrectAnswer { get; set; }

	public string? Explanation { get; set; }
	public List<string>? Hints { get; set; }
	public int Points { get; set; } = 1;
	public int OrderIndex { get; set; }
	public bool IsRequired { get; set; } = true;
	public DateTime CreatedAt { get; set; }

	public Lesson? Lesson { get; set; }
	public ICollection<TaskSubmission> Submissions { get; set; } = new List<TaskSubmission>();
}
