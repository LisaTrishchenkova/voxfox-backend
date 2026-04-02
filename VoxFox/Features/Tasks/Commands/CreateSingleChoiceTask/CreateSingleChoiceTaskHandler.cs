using System.ComponentModel.DataAnnotations;
using MediatR;
using VoxFox.Enums;
using VoxFox.Interfaces.Task;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Commands.CreateSingleChoiceTask;

public class CreateSingleChoiceTaskHandler : IRequestHandler<CreateSingleChoiceTaskCommand, TaskTeacherDto>
{
	private readonly ITaskRepository _taskRepository;

	public CreateSingleChoiceTaskHandler(ITaskRepository taskRepository)
	{
		_taskRepository = taskRepository;
	}

	public async Task<TaskTeacherDto> Handle(CreateSingleChoiceTaskCommand request, CancellationToken cancellationToken)
	{
		if (request.Options.Count < 2)
		{
			throw new ValidationException("SingleChoice должен иметь минимум 2 варианта");
		}
		if (request.CorrectIndex < 0 || request.CorrectIndex >= request.Options.Count)
			throw new ValidationException("CorrectIndex выходит за пределы Options");

		var maxOrder = await _taskRepository.GetMaxOrderIndexAsync(request.LessonId);

		var task = new TaskEntity
		{
			LessonId = request.LessonId,
			Type = TaskType.SingleChoice,
			Question = request.Question,
			Options = request.Options,
			CorrectIndex = request.CorrectIndex,
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
		CorrectIndex = task.CorrectIndex,
		Explanation = task.Explanation,
		Hints = task.Hints,
		Points = task.Points,
		OrderIndex = task.OrderIndex,
		IsRequired = task.IsRequired,
		CreatedAt = task.CreatedAt
	};
}
