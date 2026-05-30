using MediatR;

namespace VoxFox.Features.Tasks.Commands.DeleteTask;

public class DeleteTaskCommand : IRequest<bool>
{
	public Guid TaskId { get; set; }
}
