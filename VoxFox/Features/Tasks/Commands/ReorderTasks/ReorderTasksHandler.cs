using MediatR;
using VoxFox.Exception;
using VoxFox.Interfaces.Task;
using VoxFox.Models.Entities;

namespace VoxFox.Features.Tasks.Commands.ReorderTasks;

public class ReorderTasksHandler : IRequestHandler<ReorderTasksCommand>
{
	private readonly ITaskRepository _taskRepository;
	private readonly ApplicationContext _context;

	public ReorderTasksHandler(
		ITaskRepository taskRepository,
		ApplicationContext context)
	{
		_taskRepository = taskRepository;
		_context = context;
	}

	public async Task Handle(
		ReorderTasksCommand request,
		CancellationToken cancellationToken)
	{
		var tasks = await _taskRepository.GetByLessonIdAsync(request.LessonId);

		if (tasks.Count != request.TaskIds.Count)
			throw new ValidationException("Количество заданий не совпадает");

		for (int i = 0; i < request.TaskIds.Count; i++)
		{
			var task = tasks.FirstOrDefault(t => t.Id == request.TaskIds[i]);
			if (task == null)
				throw new NotFoundException(
					$"Задание с id: {request.TaskIds[i]} не найдено в этом уроке");

			task.OrderIndex = i + 1;
		}

		_context.Tasks.UpdateRange(tasks);
		await _context.SaveChangesAsync(cancellationToken);
	}
}
