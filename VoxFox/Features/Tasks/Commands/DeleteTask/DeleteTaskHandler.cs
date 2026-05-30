using MediatR;
using VoxFox.Interfaces.Task;

namespace VoxFox.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, bool>
{
	private readonly ITaskRepository _taskRepository;

	public DeleteTaskHandler(ITaskRepository taskRepository)
	{
		_taskRepository = taskRepository;
	}

	public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
	{
		var task = await _taskRepository.GetByIdAsync(request.TaskId);
		if (task == null)
			throw new KeyNotFoundException($"Задание с id {request.TaskId} не найдено");

		task.IsDeleted = true;
		await _taskRepository.UpdateAsync(task);
		return true;
	}
}
