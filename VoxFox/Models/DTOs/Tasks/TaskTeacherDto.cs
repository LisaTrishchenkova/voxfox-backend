using VoxFox.Enums;

namespace VoxFox.Models.DTOs.Tasks;

public class TaskTeacherDto
{
	public Guid Id { get; init; }
	public Guid LessonId { get; init; }
	public TaskType Type { get; init; }
	public string Question { get; init; }
	public List<string>? Options { get; init; } //варианты ответа
	public int? CorrectIndex { get; init; }
	public List<int>? CorrectIndexes { get; init; }
	public string? CorrectAnswer { get; init; }
	public string? Explanation { get; init; }
	public List<string> Hints { get; init; } //подсказки если студент застрял
	public int Points { get; init; } //очки за правильный ответ
	public int OrderIndex { get; init;  } //порядок в уроке
	public bool IsRequired { get; init; }
	public DateTime CreatedAt { get; init; }
}
