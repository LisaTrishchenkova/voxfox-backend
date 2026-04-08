using System.ComponentModel.DataAnnotations;
using MediatR;
using VoxFox.Enums;
using VoxFox.Interfaces.Task;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Commands.CreateMultiChoiceTask;

public class CreateMultiChoiceTaskHandler : IRequestHandler<CreateMultiChoiceTaskCommand, TaskTeacherDto>
{
	private readonly ITaskRepository _taskRepository;

	public CreateMultiChoiceTaskHandler(ITaskRepository taskRepository)
	{
		_taskRepository = taskRepository;
	}
	public async Task<TaskTeacherDto> Handle(CreateMultiChoiceTaskCommand request, CancellationToken cancellationToken)
	{
		if (request.Options.Count < 2)
			throw new ValidationException("MultiChoice должен иметь минимум 2 варианта");

		if (request.CorrectIndexes.Count == 0)
			throw new ValidationException(
				"MultiChoice должен иметь хотя бы один правильный ответ");

		if (request.CorrectIndexes.Any(i => i < 0 || i >= request.Options.Count))
			throw new ValidationException(
				"Один из CorrectIndexes выходит за пределы Options");

		var maxOrder = await _taskRepository.GetMaxOrderIndexAsync(request.LessonId);

		var task = new TaskEntity
		{
			LessonId = request.LessonId,
			Type = TaskType.MultiChoice,
			Question = request.Question,
			Options = request.Options,
			CorrectIndexes = request.CorrectIndexes,
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
		Options = task.Options,
		CorrectIndexes = task.CorrectIndexes,
		Explanation = task.Explanation,
		Hints = task.Hints,
		Points = task.Points,
		OrderIndex = task.OrderIndex,
		IsRequired = task.IsRequired,
		CreatedAt = task.CreatedAt

	};
}
