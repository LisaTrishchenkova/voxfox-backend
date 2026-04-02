using System.ComponentModel.DataAnnotations;
using MediatR;
using VoxFox.Enums;
using VoxFox.Interfaces.Task;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Commands.CreateTextInputTask;

public class CreateTextInputTaskHandler : IRequestHandler<CreateTextInputTaskCommand, TaskTeacherDto>
{
	private readonly ITaskRepository _taskRepository;

	public CreateTextInputTaskHandler(ITaskRepository taskRepository)
	{
		_taskRepository = taskRepository;
	}
	public async Task<TaskTeacherDto> Handle(CreateTextInputTaskCommand request, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.CorrectAnswer))
			throw new ValidationException("TextInput требует CorrectAnswer");

		var maxOrder = await _taskRepository.GetMaxOrderIndexAsync(request.LessonId);

		var task = new TaskEntity
		{
			LessonId = request.LessonId,
			Type = TaskType.TextInput,
			Question = request.Question,
			CorrectAnswer = request.CorrectAnswer,
			Explanation = request.Explanation,
			Hints = request.Hints,
			Points = request.Points,
			IsRequired = request.IsRequired,
			OrderIndex = maxOrder + 1,
			CreatedAt = DateTime.UtcNow
		};

		var created = await _taskRepository.AddAsync(task);
		return MapToDto(created);
	}

	private static TaskTeacherDto MapToDto(TaskEntity task) => new()
	{
		Id = task.Id,
		LessonId = task.LessonId,
		Type = task.Type,
		Question = task.Question,
		CorrectAnswer = task.CorrectAnswer,
		Explanation = task.Explanation,
		Hints = task.Hints,
		Points = task.Points,
		OrderIndex = task.OrderIndex,
		IsRequired = task.IsRequired,
		CreatedAt = task.CreatedAt
	};
}
