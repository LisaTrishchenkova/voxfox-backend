using MediatR;
using VoxFox.Exception;
using VoxFox.Interfaces.Task;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Queries.GetTaskById;

public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, object>
{
	private readonly ITaskRepository _taskRepository;

	public GetTaskByIdHandler(ITaskRepository taskRepository)
	{
		_taskRepository = taskRepository;
	}

	public async Task<object> Handle(
		GetTaskByIdQuery request,
		CancellationToken cancellationToken)
	{
		var task = await _taskRepository.GetByIdAsync(request.TaskId);
		if (task == null)
			throw new NotFoundException($"Задание с id: {request.TaskId} не найдено");

		return request.IsTeacher
			? MapToTeacherDto(task)
			: MapToStudentDto(task);
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
