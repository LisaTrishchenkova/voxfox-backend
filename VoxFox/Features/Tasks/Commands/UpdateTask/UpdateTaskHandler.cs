using MediatR;
using VoxFox.Enums;
using VoxFox.Exception;
using VoxFox.Interfaces.Task;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Commands.UpdateTask;

public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, TaskTeacherDto>
{
	private readonly ITaskRepository _taskRepository;

	public UpdateTaskHandler(ITaskRepository taskRepository)
	{
		_taskRepository = taskRepository;
	}

	public async Task<TaskTeacherDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
	{
		var task = await _taskRepository.GetByIdAsync(request.TaskId);
		if (task == null)
			throw new KeyNotFoundException($"Задание с id {request.TaskId} не найдено");

		if (request.Question != null)
			task.Question = request.Question;

		if (request.Explanation != null)
			task.Explanation = request.Explanation;

		if (request.Hints != null)
			task.Hints = request.Hints;

		if (request.Points.HasValue)
			task.Points = request.Points.Value;

		if (request.IsRequired.HasValue)
			task.IsRequired = request.IsRequired.Value;

		switch (task.Type)
		{
			case TaskType.SingleChoice:
				if (task.Type == TaskType.SingleChoice && (request.CorrectAnswer != null || request.CorrectIndexes != null))
					throw new ValidationException("Для SingleChoice нельзя передавать correctAnswer и correctIndexes");
				if (request.Options != null)
				{
					if (request.Options.Count < 2)
						throw new ValidationException("SingleChoice должен иметь минимум 2 варианта");
					task.Options = request.Options;
				}

				if (request.CorrectIndex.HasValue)
				{
					var opts = request.Options ?? task.Options;
					if (request.CorrectIndex < 0 || request.CorrectIndex >= opts?.Count)
						throw new ValidationException("CorrectIndex выходит за пределы Options");
					task.CorrectIndex = request.CorrectIndex;
				}

				break;

			case TaskType.MultiChoice:
				if (task.Type == TaskType.MultiChoice && (request.CorrectAnswer != null || request.CorrectIndex.HasValue))
					throw new ValidationException("Для MultiChoice нельзя передавать correctAnswer и correctIndex");
				if (request.Options != null)
				{
					if (request.Options.Count < 2)
						throw new ValidationException("MultiChoice должен иметь минимум 2 варианта");
					task.Options = request.Options;
				}

				if (request.CorrectIndexes != null)
				{
					var opts = request.Options ?? task.Options;
					if (request.CorrectIndexes.Any(i => i < 0 || i >= opts?.Count))
						throw new ValidationException("CorrectIndexes выходят за пределы Options");
					task.CorrectIndexes = request.CorrectIndexes;
				}

				break;

			case TaskType.TextInput:
				if (task.Type == TaskType.TextInput && (request.Options != null || request.CorrectIndex.HasValue || request.CorrectIndexes != null))
					throw new ValidationException("Для TextInput нельзя обновлять options и correctIndex");
				if (request.CorrectAnswer != null)
				{
					if (string.IsNullOrWhiteSpace(request.CorrectAnswer))
						throw new ValidationException("CorrectAnswer не может быть пустым");
					task.CorrectAnswer = request.CorrectAnswer;
				}

				break;
		}

		var updated = await _taskRepository.UpdateAsync(task);
		return MapToDto(updated);
	}

	private static TaskTeacherDto MapToDto(TaskEntity task) => new()
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
}
