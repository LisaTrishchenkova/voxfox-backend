using MediatR;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Commands.CreateMultiChoiceTask;

public class CreateMultiChoiceTaskCommand : IRequest<TaskTeacherDto>
{
	public Guid LessonId { get; init; }
	public string Question { get; init; } = null!;
	public List<string> Options { get; init; } = new();
	public List<int> CorrectIndexes { get; init; } = new();
	public string? Explanation { get; init; }
	public List<string>? Hints { get; init; }
	public int Points { get; init; } = 1;
	public bool IsRequired { get; init; } = true;
}
