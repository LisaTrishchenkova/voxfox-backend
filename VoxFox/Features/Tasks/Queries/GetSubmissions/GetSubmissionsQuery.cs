using MediatR;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Queries.GetSubmissions;

public class GetSubmissionsQuery : IRequest<IList<TaskSubmissionDto>>
{
	public Guid TaskId { get; init; }
}
