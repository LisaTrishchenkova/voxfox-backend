using MediatR;
using VoxFox.Interfaces.Task;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Queries.GetTasksByLesson;

public class GetTasksByLessonHandler : IRequestHandler<GetTasksByLessonQuery, IList<object>>
{
	private readonly ITaskRepository _taskRepository;

	public GetTasksByLessonHandler(ITaskRepository taskRepository)
	{
		_taskRepository = taskRepository;
	}

	public async Task<IList<object>> Handle(
		GetTasksByLessonQuery request,
		CancellationToken cancellationToken)
	{
		var tasks = await _taskRepository.GetByLessonIdAsync(request.LessonId);

		return request.IsTeacher
			? tasks.Select(t => (object)MapToTeacherDto(t)).ToList()
			: tasks.Select(t => (object)MapToStudentDto(t)).ToList();
	}

	private static TaskTeacherDto MapToTeacherDto(TaskEntity task) => new()
	{
		Id = task.Id,
		LessonId = task.LessonId,
		Type = task.Type,
		Question = task.Question,
		Options = task.Options,
		CorrectIndex = task.CorrectIndex,
		CorrectIndexes = task.CorrectIndexes,
		CorrectAnswer = task.CorrectAnswer,
		Explanation = task.Explanation,
		Hints = task.Hints,
		Points = task.Points,
		OrderIndex = task.OrderIndex,
		IsRequired = task.IsRequired,
		CreatedAt = task.CreatedAt
	};

	private static TaskStudentDto MapToStudentDto(TaskEntity task) => new()
	{
		Id = task.Id,
		LessonId = task.LessonId,
		Type = task.Type,
		Question = task.Question,
		Options = task.Options,
		Hints = task.Hints,
		Points = task.Points,
		OrderIndex = task.OrderIndex,
		IsRequired = task.IsRequired
	};
}
