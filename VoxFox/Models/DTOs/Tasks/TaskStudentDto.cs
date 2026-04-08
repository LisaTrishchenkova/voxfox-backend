using VoxFox.Enums;

namespace VoxFox.Models.DTOs.Tasks;

public class TaskStudentDto
{
	public Guid Id { get; init; }
	public Guid LessonId { get; init; }
	public TaskType Type { get; init; }
	public string Question { get; init; } = null!;
	public List<string>? Options { get; init; } //варианты ответа
	public List<string> Hints { get; init; } //подсказки если студент застрял
	public int Points { get; init; } //очки за правильный ответ
	public int OrderIndex { get; init;  } //порядок в уроке
	public bool IsRequired { get; init; }
}
