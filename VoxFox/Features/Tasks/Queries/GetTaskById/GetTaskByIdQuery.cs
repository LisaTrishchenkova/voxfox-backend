using MediatR;

namespace VoxFox.Features.Tasks.Queries.GetTaskById;

public class GetTaskByIdQuery : IRequest<object>
{
	public Guid TaskId { get; init; }
	public bool IsTeacher { get; init; }
}
